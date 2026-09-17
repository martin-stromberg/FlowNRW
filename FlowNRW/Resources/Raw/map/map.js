/* Local rendering only. Network requests are exclusively validated by the native tile gateway. */
'use strict';
let map, layer, session, serial=0, failed=false, queue=[], active=0, resetting=false;
const waiting=new Map();
const send=data=>window.HybridWebView.SendRawMessage(JSON.stringify(data));
function pump(){if(resetting)return;while(active<4&&queue.length){const item=queue.shift();if(!waiting.has(item.id))continue;active++;waiting.get(item.id).active=true;send(item);}}
function pointText(value){const node=document.createElement('span');node.textContent=value;return node;}
window.receiveMap=function(data){
 if(data.type==='tile'){
  if(data.session!==session)return;
  const item=waiting.get(data.id);if(!item)return;
  waiting.delete(data.id);active=Math.max(0,active-1);
  if(data.error){failed=true;send({type:'failed',session});}
  if(data.data){item.image.onload=()=>item.done(null,item.image);item.image.onerror=()=>{failed=true;send({type:'failed',session});item.done(new Error('image'),item.image);};item.image.src=data.data;}
  else item.done(new Error('tile'),item.image);
  pump();return;
 }
 if(data.type!=='init')return;
 resetting=true;if(map)map.remove();
 session=data.session;failed=false;queue=[];waiting.clear();active=0;resetting=false;
 map=L.map('map',{attributionControl:false,minZoom:data.minZoom,maxZoom:data.maxZoom});
 map.on('moveend',()=>send({type:'view',session,lat:map.getCenter().lat,lon:map.getCenter().lng,zoom:map.getZoom()}));
 const NativeTiles=L.GridLayer.extend({createTile:function(coords,done){
  const image=document.createElement('img');image.alt='';image.width=256;image.height=256;
  const id=++serial;image.dataset.tileId=String(id);waiting.set(id,{image,done,active:false});queue.push({type:'tile',session,id,z:coords.z,x:coords.x,y:coords.y});queueMicrotask(pump);return image;
 }});
 layer=new NativeTiles({tileSize:256,keepBuffer:0,noWrap:true,minZoom:data.minZoom,maxZoom:data.maxZoom});
 layer.on('tileunload',event=>{const id=Number(event.tile.dataset.tileId),item=waiting.get(id);if(!item)return;waiting.delete(id);if(item.active){active=Math.max(0,active-1);send({type:'cancel',session,id});}pump();});
 layer.on('load',()=>send({type:failed?'failed':'loaded',session}));layer.addTo(map);
 const bounds=[];
 for(const marker of data.markers){
  const position=[marker.lat,marker.lon];bounds.push(position);
  const pin=L.marker(position,{icon:L.divIcon({className:'station',html:String(marker.id+1),iconSize:[38,38],iconAnchor:[19,19]}),title:marker.label,alt:'Haltestelle '+(marker.id+1)+': '+marker.label,keyboard:true});
  pin.bindTooltip(pointText(marker.label));pin.on('click',()=>send({type:'select',session,id:marker.id}));pin.addTo(map);
 }
 const colors=['#1754a1','#a43b00','#00735c','#792ba0'];
 data.segments.forEach((segment,index)=>{L.polyline(segment.points,{color:colors[index%colors.length],weight:5,dashArray:segment.label==='Fußweg'?'6 8':null}).bindTooltip(pointText(segment.label)).addTo(map);bounds.push(...segment.points);});
 for(const point of data.endpoints){L.circleMarker(point,{radius:5,color:'#222',fillColor:'#fff',fillOpacity:1}).addTo(map);bounds.push(point);}
 if(bounds.length)map.fitBounds(bounds,{padding:[35,35],maxZoom:15});else map.setView([51.1657,10.4515],6);
};
window.addEventListener('load',()=>send({type:'ready'}));




