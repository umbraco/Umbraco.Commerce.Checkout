
export type InstallUccApiResponse = {
    success: boolean,
    message?: string
}
export const installUmbracoCommerceCheckoutAsync: (siteRootNodeId: string) => Promise<InstallUccApiResponse> = (siteRootNodeId: string) => {
    const response = fetch('/umbraco/management/api/v1/umbraco-commerce-checkout/install?siteRootNodeId=' + siteRootNodeId, {
        credentials: 'include',
    })
        .then(
            (response: Response) => {
                return response.json();
            },
            (reason: unknown) => ({
                success: false,
                message: JSON.stringify(reason),
            })) as Promise<InstallUccApiResponse>;

    return response;
};
