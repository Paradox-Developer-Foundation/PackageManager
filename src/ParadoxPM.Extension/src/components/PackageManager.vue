<template>
    <div class="container">
        <div class="left-panel">
            <vscode-textfield class="search-box" placeholder="搜索包..." style="width: 100%; box-sizing: border-box;">
                <vscode-icon slot="content-before" name="search" title="search"></vscode-icon>
            </vscode-textfield>
            <ListBox :items="packages" :show-tooltip="false" @selection-changed="selectedPackage = $event">
                <template #item="{ item }">
                    <div class="package-item">
                        <p>{{ item.name }} {{ item.versions.length > 0 ? item.versions[0].version || "" : "" }}</p>
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
    /* 两个相同大小的列 */
    height: 100vh;
    /* 全屏高度 */
    gap: 16px;
    /* 间距 */
}

.left-panel,
.right-panel {
    border: 1px solid #ccc;
    padding: 4px;
    overflow-y: auto;
    /* 内容溢出时滚动 */
}

.package-item {
    padding: 1px;
    border-bottom: 1px;
    cursor: pointer;
}
</style>