
export type InstallUccApiResponse = {
    success: boolean,
    message?: string
}

export const OpenApiConfig: {
    credentials: RequestCredentials,
    token: () => Promise<string | undefined>,
} = {
    credentials: 'same-origin',
    token: async () => '',
};

export const installUmbracoCommerceCheckoutAsync: (siteRootNodeId: string) => Promise<InstallUccApiResponse> = async (siteRootNodeId: string) => {
    const response = fetch('/umbraco/management/api/v1/umbraco-commerce-checkout/install?siteRootNodeId=' + siteRootNodeId, {
        credentials: OpenApiConfig.credentials,
        headers: {
            'Authorization': 'Bearer ' + await OpenApiConfig.token(),
        },
    }).then(
        (response: Response) => {
            return response.json() as Promise<InstallUccApiResponse>;
        },
        (reason: unknown) => ({
            success: false,
            message: JSON.stringify(reason),
        })) as Promise<InstallUccApiResponse>;

    return response;
};
