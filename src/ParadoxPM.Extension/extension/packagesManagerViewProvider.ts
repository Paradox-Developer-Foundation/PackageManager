import * as vscode from "vscode";
import { WebviewHelpers } from "./WebviewHelpers.js";

export class PackagesManagerViewProvider implements vscode.WebviewViewProvider {
  public static readonly viewType = "packages-manager";

  private _view?: vscode.WebviewView;
  private _context: vscode.ExtensionContext;

  public constructor(context: vscode.ExtensionContext) {
    this._context = context;
  }

  resolveWebviewView(
    webviewView: vscode.WebviewView,
    context: vscode.WebviewViewResolveContext,
    token: vscode.CancellationToken
  ): Thenable<void> | void {
    webviewView.webview.options = {
      enableScripts: true,
    };
    this._view = webviewView;

    webviewView.webview.html = WebviewHelpers.getHtml(
      webviewView.webview,
      this._context,
      "packageManager"
    );
  }
}
