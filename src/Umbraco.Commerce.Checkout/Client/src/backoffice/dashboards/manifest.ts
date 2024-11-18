import type { ManifestDashboard, ManifestModal } from '@umbraco-cms/backoffice/extension-registry';
import { UCC_INSTALLER_MODAL_ALIAS } from '../modals/installer-modal.token';

const dashboardManifests: Array<ManifestDashboard | ManifestModal> = [
    {
        type: 'dashboard',
        alias: 'Umbraco.Commerce.Checkout.InstallerDashboard',
        weight: -100,
        name: 'Umbraco Commerce Checkout v15',
        meta: {
        },
        element: () => import('./installer-dashboard.element'),
        elementName: 'uc-checkout-installer-dashboard',
        conditions: [
            {
                'alias': 'Umb.Condition.SectionAlias',
                'match': 'Umb.Section.Settings',
            },
        ],
    },
    {
        type: 'modal',
        alias: UCC_INSTALLER_MODAL_ALIAS,
        meta: {},
        name: 'Umbraco Commerce Checkout Installer Modal',
        elementName: 'ucc-installer-config-modal',
        element: () => import('../modals/installer-modal.element'),
    },
];
export const manifests = [...dashboardManifests];
