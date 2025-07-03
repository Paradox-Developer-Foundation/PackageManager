<template>
    <div class="container">
        <div class="left-panel">
            <div class="search-header">
                <vscode-textfield class="search-box" placeholder="搜索包...">
                    <vscode-icon slot="content-before" name="search" title="search"></vscode-icon>
                </vscode-textfield>
            </div>
            <ListBox :items="packages" :show-tooltip="false" @selection-changed="selectedPackage = $event">
                <template #item="{ item }">
                    <div class="package-item">
                        <!-- TODO: 显示最新版本 -->
                        <p style="margin-left: 4px;">{{ item.name }} {{ item.versions.length > 0 ?
                            item.versions[0].version || "" : "" }}</p>
                    </div>
                </template>
            </ListBox>
        </div>

        <div class="right-panel">
            <PackageDetails :package="selectedPackage" />
        </div>
    </div>
</template>

<script lang="ts" setup>
import ListBox from "./ListBox.vue";
import { onMounted, ref } from "vue";
import type { PackageInfo } from "../types/packageInfo";
import { ApiResponse } from "../types/apiResponse";
import PackageDetails from "./PackageDetails.vue";

const packages = ref<PackageInfo[]>([]);
const selectedPackage = ref<PackageInfo | null>(null);

onMounted(() => {
    fetchPackages();
});

const fetchPackages = async (): Promise<void> => {
    try {
        const response = await fetch("https://localhost:7295/api/packages", {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            },
        });
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const result: ApiResponse<PackageInfo[]> = await response.json();
        packages.value = result.data;
    } catch (error) {
        console.error("获取包列表失败:", error);
    }
};
</script>

<style scoped>
.container {
    display: grid;
    grid-template-columns: 1fr 1fr;
    height: 100vh;
    gap: 16px;
}

.search-header {
    flex-shrink: 0;
    /* 防止搜索框收缩 */
    padding: 2px;
    background-color: var(--vscode-editor-background);
}

.search-box {
    width: 100%;
    box-sizing: border-box;
}

.left-panel {
    display: flex;
    flex-direction: column;
    border-right: 1px solid var(--vscode-panel-border);
    /* 防止整个面板滚动 */
    overflow: hidden;
}

.right-panel {
    padding: 4px;
    overflow-y: auto;
}

.package-item {
    padding: 0.5px;
    border-bottom: 1px solid var(--vscode-panel-border);
    cursor: pointer;
    transition: background-color 0.2s ease;
}

.package-item:hover {
    background-color: var(--vscode-list-hoverBackground);
}
</style>