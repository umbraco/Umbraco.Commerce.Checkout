import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

import { manifests as dashboardManifest } from './dashboards/manifest';

const manifests: Array<UmbExtensionManifest> = [
    ...dashboardManifest,
];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
    extensionRegistry.registerMany(manifests);
};
