import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

import { UMB_AUTH_CONTEXT } from '@umbraco-cms/backoffice/auth';
import { manifests as dashboardManifest } from './dashboards/manifest';
import { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';
import { OpenApiConfig } from './apis/install.api';

const manifests: Array<ManifestTypes> = [
    ...dashboardManifest,
];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
    extensionRegistry.registerMany(manifests);
    _host.consumeContext(UMB_AUTH_CONTEXT, async (instance) => {
        if (!instance) return;
        const umbOpenApi = instance.getOpenApiConfiguration();
        OpenApiConfig.token = umbOpenApi.token;
    });
};