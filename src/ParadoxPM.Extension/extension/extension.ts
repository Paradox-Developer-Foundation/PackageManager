import * as vscode from "vscode";
import { PackagesManagerViewProvider } from "./packagesManagerViewProvider.js";

export function activate(context: vscode.ExtensionContext) {
  console.log('Congratulations, your extension "ParadoxPM-Server" is now active!');

  const provider = new PackagesManagerViewProvider(context);

  context.subscriptions.push(
    vscode.window.registerWebviewViewProvider(PackagesManagerViewProvider.viewType, provider)
  );
}

export function deactivate() {}
