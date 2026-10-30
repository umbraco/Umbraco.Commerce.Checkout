import { umbHttpClient } from '@umbraco-cms/backoffice/http-client';

export type InstallUccApiResponse = {
    success: boolean,
    message?: string
}

export const installUmbracoCommerceCheckoutAsync: (siteRootNodeId: string) => Promise<InstallUccApiResponse> = async (siteRootNodeId: string) => {
    try {
        const result = await umbHttpClient.post({
            url: '/umbraco/management/api/v1/umbraco-commerce-checkout/install',
            query: { siteRootNodeId },
        });

        if (result.error !== undefined) {
            return {
                success: false,
                message: typeof result.error === 'string' ? result.error : JSON.stringify(result.error),
            };
        }

        // The client types an untyped response as unknown; the endpoint returns { success, message }
        return result.data as InstallUccApiResponse;
    } catch (reason: unknown) {
        return {
            success: false,
            message: reason instanceof Error ? reason.message : JSON.stringify(reason),
        };
    }
};
