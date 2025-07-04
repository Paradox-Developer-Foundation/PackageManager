import vscode from '@tomjs/vite-plugin-vscode';
import vue from '@vitejs/plugin-vue';
import { resolve } from 'path';
import { defineConfig } from 'vite';

export default defineConfig({
  plugins: [
    vue({
      template: {
        compilerOptions: {
          isCustomElement: (tag: string) => tag.startsWith('vscode-'),
        },
      },
    }),
    vscode({
      extension: { entry: "extension/extension.ts" }
    }),
  ],
  build: {
    rollupOptions: {
      input: {
        packageManager: resolve(__dirname, "src", "html", "packageManager.html")
      },
    },
  },
});