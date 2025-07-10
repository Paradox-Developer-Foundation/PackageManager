import type { ExtensionContext, Webview } from "vscode";

export class WebviewHelpers {
  static getHtml(webview: Webview, context: ExtensionContext, input: string) {
    return __getWebviewHtml__({
      serverUrl: `${process.env.VITE_DEV_SERVER_URL}src/html/${input}.html`,
      webview,
      context,
      inputName: input,
    });
  }
}
