import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

import { manifests as dashboardManifest } from './dashboards/manifest';

import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const manifests: Array<ManifestTypes> = [
    ...dashboardManifest,
];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
    extensionRegistry.registerMany(manifests);
};