import { execSync } from 'child_process';
import { copyFileSync, readFileSync, writeFileSync } from 'fs';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

const __filename = fileURLToPath(import.meta.url);
const __dirname = dirname(__filename);

function run(command, description) {
  console.log(`\n→ ${description}...`);
  try {
    execSync(command, { stdio: 'inherit', cwd: __dirname });
    console.log(`✓ ${description} completed`);
  } catch (error) {
    console.error(`✗ ${description} failed`);
    throw error;
  }
}

console.log('Building Single Executable Application for Node.js...\n');

// Step 1: Bundle the application with esbuild - fully bundled
run(
  'npx esbuild format-liquid.js --bundle --platform=node --format=cjs --outfile=format-liquid.bundle.js',
  'Bundling application with esbuild'
);

// Step 2: Patch the bundle to handle module-specific issues
console.log('\n→ Patching bundle for SEA compatibility...');
let bundleContent = readFileSync(join(__dirname, 'format-liquid.bundle.js'), 'utf8');

// Add shims at the top
const shimsContent = `(function() {
// SEA Compatibility Shims
const { createRequire } = require('module');
const { fileURLToPath: originalFileURLToPath } = require('url');
const _path = require('path');

// Create a virtual __filename and __dirname for the SEA environment
const virtualFilename = _path.join(process.cwd(), 'format-liquid.bundle.js');
const virtualDirname = process.cwd();

// Patch createRequire to use our virtual filename
const originalCreateRequire = createRequire;
require('module').createRequire = function(filename) {
  // If filename is undefined or not useful, use our virtual one
  if (!filename || filename === 'undefined.js' || filename === '.') {
    filename = virtualFilename;
  }
  return originalCreateRequire(filename);
};

// Patch fileURLToPath to handle undefined import.meta.url
const originalURLModule = require('url');
originalURLModule.fileURLToPath = function(url) {
  if (url === undefined || url === null) {
    return virtualFilename;
  }
  return originalFileURLToPath(url);
};

// Set globals for any code that checks them
if (typeof global.__filename === 'undefined') {
  global.__filename = virtualFilename;
}
if (typeof global.__dirname === 'undefined') {
  global.__dirname = virtualDirname;
}
})();

`;

bundleContent = shimsContent + bundleContent;
writeFileSync(join(__dirname, 'format-liquid.bundle.js'), bundleContent);
console.log('✓ Bundle patched for SEA compatibility');

// Step 3: Generate the blob to be injected
run(
  'node --experimental-sea-config sea-config.json',
  'Generating SEA blob'
);

// Step 4: Copy the node executable
console.log('\n→ Copying Node.js executable...');
const nodePath = process.execPath;
const outputPath = join(__dirname, 'formatter.exe');
copyFileSync(nodePath, outputPath);
console.log('✓ Node.js executable copied');

// Step 5: Inject the blob
if (process.platform === 'win32') {
  run(
    'npx postject formatter.exe NODE_SEA_BLOB sea-prep.blob --sentinel-fuse NODE_SEA_FUSE_fce680ab2cc467b6e072b8b5df1996b2 --overwrite',
    'Injecting application blob into executable'
  );
} else {
  run(
    'npx postject formatter.exe NODE_SEA_BLOB sea-prep.blob --sentinel-fuse NODE_SEA_FUSE_fce680ab2cc467b6e072b8b5df1996b2 --macho-segment-name NODE_SEA',
    'Injecting application blob into executable'
  );
}

console.log('\n✅ Build completed successfully! formatter.exe is ready.\n');
