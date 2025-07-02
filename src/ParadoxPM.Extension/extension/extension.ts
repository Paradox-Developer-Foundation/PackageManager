import * as vscode from 'vscode';

export function activate(context: vscode.ExtensionContext) {
	console.log('Congratulations, your extension "ParadoxPM-Server" is now active!');

	const disposable = vscode.commands.registerCommand('ParadoxPM-Server.helloWorld', () => {
		vscode.window.showInformationMessage('Hello World from ParadoxPM.Server!');
	});

	context.subscriptions.push(disposable);
}

export function deactivate() {}
