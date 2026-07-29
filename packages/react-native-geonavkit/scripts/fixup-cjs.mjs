// The root package.json declares "type": "module", so Node would otherwise
// parse dist/cjs/*.js as ESM. A nested package.json scopes that directory back
// to CommonJS.
import { writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const pkgRoot = dirname(dirname(fileURLToPath(import.meta.url)));
writeFileSync(
  join(pkgRoot, 'dist', 'cjs', 'package.json'),
  JSON.stringify({ type: 'commonjs' }, null, 2) + '\n'
);
