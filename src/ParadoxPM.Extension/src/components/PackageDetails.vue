<template>
    <div class="package-details">
        <div class="package-details-header">
            <p>版本</p>
            <vscode-single-select :selectedIndex="selectedIndex">
                <vscode-option v-for="version in package?.versions">{{ version.version }}</vscode-option>
            </vscode-single-select>
        </div>
        <div v-if="package" class="package-details-content">
            <p><strong>名称:</strong> {{ package.name }}</p>
            <p><strong>描述:</strong> {{ package.description || "无描述" }}</p>
            <p><strong>作者:</strong> {{ package.author || "未知" }}</p>
            <p><strong>许可证:</strong> {{ package.license || "未知" }}</p>
            <div class="links-container">
                <span>链接:</span>
                <a class="link" :href="package.repository" v-if="package.repository">仓库地址</a>
                <a class="link" :href="package.homepage" v-if="package.homepage">项目主页</a>
            </div>
            <div v-if="package.versions[selectedIndex]?.dependencies && package.versions[selectedIndex].dependencies.length > 0">
                <h2>依赖</h2>
                <p v-for="(dependency, index) in package.versions[selectedIndex].dependencies" :key="index">
                    {{ dependency.normalizedName }} >= {{ dependency.minVersion }}
                </p>
            </div>
            <h2 v-else>
                无依赖
            </h2>
        </div>
        <div v-else>
            <p>请选择一个包以查看详情。</p>
        </div>
    </div>
</template>

<script lang="ts" setup>
import { ref } from 'vue';
import type { PackageInfo } from '../types/packageInfo';

const selectedIndex = ref(0);

defineProps<{
    package: PackageInfo | null
}>()

</script>

<style scoped>
.package-details-header {
    display: flex;
    align-items: center;
    justify-content: left;
    gap: 4px;
}

.links-container {
    display: flex;
    align-items: center;
    gap: 4px;
}

.link {
    text-decoration: none;
}

.link:hover {
    text-decoration: underline;
}
</style>