import { createApp } from 'vue';
import PackageManager from '../components/PackageManager.vue';
import '@vscode-elements/elements';

// 创建并挂载Vue应用
const app = createApp(PackageManager);
app.mount('#app');
