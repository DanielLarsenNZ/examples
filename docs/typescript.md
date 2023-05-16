# How to Typescript 💐

Setting up...

[How to set up TypeScript with Node.js and Express](https://blog.logrocket.com/how-to-set-up-node-typescript-express/)

Docker...

[Dockerizing a Node.js web app](https://nodejs.org/en/docs/guides/nodejs-docker-webapp/)

## Getting started

> Based on [How to set up TypeScript with Node.js and Express](https://blog.logrocket.com/how-to-set-up-node-typescript-express/) by Aman Mittal.

```bash
npm init
npm i -D typescript @types/node
npx tsc --init

npm install -D concurrently nodemon
```

### tsconfig.json

```json
{
  "compilerOptions": {
    // ...
    "sourceMap": true,
    "outDir": "./dist",
    // ...
  }
}
```

### package.json scripts

```json
{
  "scripts": {
    "build": "npx tsc",
    "start": "node dist/index.js",
    "dev": "concurrently \"npx tsc --watch\" \"nodemon -q dist/index.js\""
  }
}
```

### .gitignore

```
node_modules
```

### VS Code launch.json

Based on [Debugging TypeScript](https://code.visualstudio.com/docs/typescript/typescript-debugging)

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "type": "node",
            "request": "launch",
            "name": "Launch Program",
            "skipFiles": [
                "<node_internals>/**"
            ],
            "program": "${workspaceFolder}\\index.ts",
            "preLaunchTask": "tsc: build - tsconfig.json",
            "outFiles": [
                "${workspaceFolder}/**/*.js"
            ]
        }
    ]
}
```