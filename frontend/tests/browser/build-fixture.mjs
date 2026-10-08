import { build } from 'vite';
import { resolve, sep } from 'node:path';
const output = process.env.EC_V01_ASSETS;
if (!output || !resolve(output).startsWith(resolve(process.env.TEMP) + sep)) throw new Error('Temporary output required');
await build({ build: { outDir: output, emptyOutDir: true, rollupOptions: { input: resolve('tests/browser/identity-entry.html') } } });
