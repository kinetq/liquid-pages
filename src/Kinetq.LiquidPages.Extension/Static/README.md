# Liquid Formatter - Single Executable Application

This directory contains a Liquid template formatter packaged as a standalone Windows executable using Node.js Single Executable Applications (SEA).

## Overview

The `formatter.exe` is a self-contained executable that formats Liquid HTML templates using Prettier and the Shopify Liquid plugin. It requires no Node.js installation to run.

## Building

To rebuild the executable:

```bash
npm install
npm run build
```

### Build Process

The build uses the official Node.js SEA (Single Executable Applications) feature:

1. **Bundle** - `esbuild` bundles all dependencies into a single CommonJS file
2. **Patch** - Custom shims are added to handle ES module compatibility issues
3. **Generate Blob** - Node.js generates an SEA blob from the bundle
4. **Copy Node** - A copy of the Node.js executable is created
5. **Inject** - `postject` injects the blob into the executable

### Files

- `format-liquid.js` - Source formatter script
- `build-sea.mjs` - Build script that orchestrates the SEA creation
- `sea-config.json` - Configuration for Node.js SEA generation
- `package.json` - Dependencies and build script

## Usage

The formatter reads from stdin and writes to stdout:

```bash
echo "<div   >test</div>" | formatter.exe
# Output: <div>test</div>

echo "{% if true %}<div>test</div>{% endif %}" | formatter.exe
# Output:
# {% if true %}
#   <div>test</div>
# {% endif %}
```

## Technical Details

- **Size**: ~79 MB (includes full Node.js runtime + Prettier + Liquid plugin)
- **Target**: Windows x64
- **Node.js Version**: Uses the version installed on the build machine
- **Format**: CommonJS bundle with ES module shims for compatibility

### Compatibility Shims

The build process adds runtime shims to handle:
- `import.meta.url` being undefined in CommonJS/SEA context
- `createRequire` with undefined filenames
- `fileURLToPath` with undefined URLs
- Global `__filename` and `__dirname` in SEA environment

## Dependencies

- **prettier**: Code formatter
- **@shopify/prettier-plugin-liquid**: Liquid template support
- **esbuild**: JavaScript bundler
- **postject**: Binary injection tool for SEA

## References

- [Node.js Single Executable Applications](https://nodejs.org/api/single-executable-applications.html)
- [Prettier](https://prettier.io/)
- [Shopify Liquid Plugin](https://github.com/Shopify/prettier-plugin-liquid)
