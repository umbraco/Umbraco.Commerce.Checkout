import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

import { manifests as dashboardManifest } from './dashboards/manifest';

import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const manifests: Array<ManifestTypes> = [
    ...dashboardManifest,
];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
    console.log('%c Checkout v14 plugin loaded ＼（〇_ｏ）／', 'font-size: 20pt;');
    extensionRegistry.registerMany(manifests);
};