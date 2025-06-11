import { UmbModalToken } from '@umbraco-cms/backoffice/modal';

// export type UccInstallerModalOpenData = object

export type UccInstallerModalSubmitValue = {
    selected: string;
}

export const UCC_INSTALLER_MODAL_ALIAS = 'Umbraco.Commerce.Checkout.InstallerConfigModal';

export const UCC_INSTALLER_MODAL_TOKEN = new UmbModalToken<object, UccInstallerModalSubmitValue>(
    UCC_INSTALLER_MODAL_ALIAS, {
    modal: {
        type: 'sidebar',
        size: 'small',
    },
});
