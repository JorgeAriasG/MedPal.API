#!/usr/bin/env node
const path = require('path');
const { execSync } = require('child_process');

// El script principal está en la raíz del monorepo
const rootScript = path.resolve(__dirname, '..', '..', '..', '..', '..', 'jira-workflow-automation.js');

const args = process.argv.slice(2);
if (!args.length) {
  console.error('Usage: node scripts/jira-cli.js <command> [args]');
  process.exit(1);
}

const cmd = `node "${rootScript}" ${args.map(a => '"' + a + '"').join(' ')}`;
try {
  const output = execSync(cmd, { stdio: 'inherit' });
  process.exit(0);
} catch (error) {
  console.error('Command failed:', error.message);
  process.exit(error.status || 1);
}
