/* Local rendering only. Network requests are exclusively validated by the native tile gateway. */
'use strict';
const TILE_TIMEOUT=15000,WATCHDOG_INTERVAL=15000;
let map, layer, session, loadId=0, serial=0, failed=false, queue=[], active=0, resetting=false, loadDone=false, lastProgress=0, watchdog=null;
const waiting=new Map();
const send=data=>window.HybridWebView.SendRawMessage(JSON.stringify(data));
function progress(){lastProgress=Date.now();}
function clearWatchdog(){if(watchdog){clearInterval(watchdog);watchdog=null;}}
function announceFailed(sess,gen){if(failed||gen!==loadId)return;failed=true;clearWatchdog();send({type:'failed',session:sess,loadId:gen});}
function pump(){if(resetting)return;while(active<4&&queue.length){const item=queue.shift();if(!waiting.has(item.id))continue;active++;waiting.get(item.id).active=true;send(item);}}
function expire(id,sess,gen){
 const item=waiting.get(id);if(!item)return;
 // A tile only times out when the whole pipeline stalled: ongoing tile
 // activity re-arms the deadline so slow but progressing loads survive.
 const idle=Date.now()-lastProgress;
 if(idle<TILE_TIMEOUT){item.timer=setTimeout(()=>expire(id,sess,gen),TILE_TIMEOUT-idle);return;}
 waiting.delete(id);if(item.active)active=Math.max(0,active-1);
 send({type:'cancel',session:sess,id});announceFailed(sess,gen);
 item.done(new Error('timeout'),item.image);pump();
}
function pointText(value){const node=document.createElement('span');node.textContent=value;return node;}
window.receiveMap=function(data){
 if(data.type==='tile'){
  if(data.session!==session)return;
  const item=waiting.get(data.id);if(!item)return;
  const sess=session,gen=loadId;
  waiting.delete(data.id);clearTimeout(item.timer);active=Math.max(0,active-1);progress();
  if(data.error)announceFailed(sess,gen);
  if(data.data){item.image.onload=()=>item.done(null,item.image);item.image.onerror=()=>{announceFailed(sess,gen);item.done(new Error('image'),item.image);};item.image.src=data.data;}
  else item.done(new Error('tile'),item.image);
  pump();return;
 }
 if(data.type!=='init')return;
 resetting=true;clearWatchdog();if(map)map.remove();
 for(const item of waiting.values())clearTimeout(item.timer);
 session=data.session;loadId=data.loadId;failed=false;loadDone=false;queue=[];waiting.clear();active=0;resetting=false;
 // Layer handlers, timers and queued sends of this generation carry sess/gen as
 // fixed closure values: a late event from a layer discarded by the next init can
 // neither report the newer loadId nor disarm the fresh watchdog installed below.
 const sess=data.session,gen=data.loadId;
 progress();
 watchdog=setInterval(()=>{if(failed||loadDone||gen!==loadId){clearWatchdog();return;}if(Date.now()-lastProgress<WATCHDOG_INTERVAL)return;announceFailed(sess,gen);},WATCHDOG_INTERVAL);
 map=L.map('map',{attributionControl:false,minZoom:data.minZoom,maxZoom:data.maxZoom});
 map.on('moveend',()=>send({type:'view',session:sess,lat:map.getCenter().lat,lon:map.getCenter().lng,zoom:map.getZoom()}));
 const NativeTiles=L.GridLayer.extend({createTile:function(coords,done){
  const image=document.createElement('img');image.alt='';image.width=256;image.height=256;
  const id=++serial;image.dataset.tileId=String(id);waiting.set(id,{image,done,active:false,timer:setTimeout(()=>expire(id,sess,gen),TILE_TIMEOUT)});queue.push({type:'tile',session:sess,id,z:coords.z,x:coords.x,y:coords.y});progress();queueMicrotask(pump);return image;
 }});
 layer=new NativeTiles({tileSize:256,keepBuffer:0,noWrap:true,minZoom:data.minZoom,maxZoom:data.maxZoom});
 layer.on('tileunload',event=>{const id=Number(event.tile.dataset.tileId),item=waiting.get(id);if(!item)return;waiting.delete(id);clearTimeout(item.timer);if(item.active){active=Math.max(0,active-1);send({type:'cancel',session:sess,id});}progress();pump();});
 layer.on('load',()=>{if(gen!==loadId)return;loadDone=true;clearWatchdog();send({type:failed?'failed':'loaded',session:sess,loadId:gen});});layer.addTo(map);
 const bounds=[];
 for(const marker of data.markers){
  const position=[marker.lat,marker.lon];bounds.push(position);
  const pin=L.marker(position,{icon:L.divIcon({className:'station',html:String(marker.id+1),iconSize:[38,38],iconAnchor:[19,19]}),title:marker.label,alt:'Haltestelle '+(marker.id+1)+': '+marker.label,keyboard:true});
  pin.bindTooltip(pointText(marker.label));pin.on('click',()=>send({type:'select',session:sess,id:marker.id}));pin.addTo(map);
 }
 const colors=['#1754a1','#a43b00','#00735c','#792ba0'];
 data.segments.forEach((segment,index)=>{L.polyline(segment.points,{color:colors[index%colors.length],weight:5,dashArray:segment.label==='Fußweg'?'6 8':null}).bindTooltip(pointText(segment.label)).addTo(map);bounds.push(...segment.points);});
 for(const point of data.endpoints){L.circleMarker(point,{radius:5,color:'#222',fillColor:'#fff',fillOpacity:1}).addTo(map);bounds.push(point);}
 if(bounds.length)map.fitBounds(bounds,{padding:[35,35],maxZoom:15});else map.setView([51.1657,10.4515],6);
};
window.addEventListener('load',()=>send({type:'ready'}));
