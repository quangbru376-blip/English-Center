const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const frontend = path.resolve(__dirname, '../..');
const files = [];
function visit(directory) {
  for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
    const file = path.join(directory, entry.name);
    if (entry.isDirectory()) visit(file);
    else if (/\.(tsx?|css)$/.test(file)) files.push(file);
  }
}
visit(path.join(frontend, 'src'));
files.push(...['package.json', 'package-lock.json', 'vite.config.ts', 'tsconfig.json'].map(f => path.join(frontend, f)));
const rows = files.sort().map(file => {
  const bytes = fs.readFileSync(file), text = bytes.toString('utf8');
  return {
    file: path.relative(frontend, file).replaceAll('\\', '/'),
    sha256: crypto.createHash('sha256').update(bytes).digest('hex'),
    // Literal selectors only; dynamic JSX expressions require the inventory review.
    literalSelectors: [...text.matchAll(/\b(name|id|data-testid|aria-label)=["']([^"']*)["']/g)].map(m => ({ attribute: m[1], value: m[2] })),
    routeLiterals: [...new Set([...text.matchAll(/["'](\/(?:student|teacher|academics|admissions|accountant|admin|account|login|register|reports)[^"'\s]*)["']/g)].map(m => m[1]))],
  };
});
const output = path.resolve(frontend, '../docs/plan/UI-refresh-evidence/source-manifest.json');
fs.writeFileSync(output, JSON.stringify({ kind: 'source-freeze-not-runtime-proof', rows }, null, 2));
process.stdout.write(JSON.stringify({ sourceFiles: rows.length, output }));
