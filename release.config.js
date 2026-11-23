const path = require("node:path");

const releaseAssetPath = process.env.RELEASE_ASSET_PATH;
const releaseAssetPaths = (process.env.RELEASE_ASSET_PATHS ?? "")
  .split(/[;\n]/)
  .map((value) => value.trim())
  .filter(Boolean);
const releaseManifestPath = process.env.RELEASE_MANIFEST_PATH;
const releaseAssets = [...releaseAssetPaths, releaseAssetPath, releaseManifestPath]
  .filter(Boolean)
  .map((assetPath) => ({ path: assetPath, name: path.basename(assetPath) }));

const releasePlugins = [
  [
    "@semantic-release/commit-analyzer",
    {
      preset: "conventionalcommits",
      releaseRules: [
        { breaking: true, release: "major" },
        { type: "feat", release: "minor" },
        { type: "fix", release: "patch" },
        { type: "docs", release: false },
        { type: "refactor", release: false },
        { type: "chore", release: false }
      ]
    }
  ],
  "./scripts/verify-release-version.cjs",
  [
    "@semantic-release/release-notes-generator",
    {
      preset: "conventionalcommits"
    }
  ],
  [
    "@semantic-release/github",
    {
      assets: releaseAssets,
      // The default GITHUB_TOKEN only has `contents: write` - it cannot comment on the PR(s)
      // associated with a released commit, which is what the plugin's "success"/"fail" steps
      // otherwise attempt by default. Without this, any release whose commit has an associated
      // PR (e.g. the routine staging->main promotion PR) fails at that final comment step with
      // a GraphQL "Resource not accessible by integration" error, even though the release
      // itself was already published successfully.
      successComment: false,
      failComment: false
    }
  ]
];

const dryRunPlugins = [
  [
    "@semantic-release/commit-analyzer",
    {
      preset: "conventionalcommits",
      releaseRules: [
        { breaking: true, release: "major" },
        { type: "feat", release: "minor" },
        { type: "fix", release: "patch" },
        { type: "docs", release: false },
        { type: "refactor", release: false },
        { type: "chore", release: false }
      ]
    }
  ]
];

// "staging" is deliberately NOT listed as a semantic-release prerelease branch: RC version
// determination for staging lives in staging-ci.yml's own "version" job, which invokes
// semantic-release with a --branches override against this same config and appends the
// lowercase "-rc.N" suffix itself (project-wide tag format: vX.Y.Z / vX.Y.Z-rc.N).
module.exports = {
  branches: ["main"],
  tagFormat: "v${version}",
  plugins: process.env.RESOLVE_DRY_RUN === "true" ? dryRunPlugins : releasePlugins
};
