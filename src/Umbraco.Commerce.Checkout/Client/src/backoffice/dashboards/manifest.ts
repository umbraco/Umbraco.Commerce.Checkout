import type { ManifestDashboard } from '@umbraco-cms/backoffice/dashboard';
import type { ManifestModal } from '@umbraco-cms/backoffice/modal';
import { UCC_INSTALLER_MODAL_ALIAS } from '../modals/installer-modal.token';

const dashboardManifests: Array<ManifestDashboard | ManifestModal> = [
    {
        type: 'dashboard',
        alias: 'Umbraco.Commerce.Checkout.InstallerDashboard',
        weight: -100,
        name: 'Umbraco Commerce Checkout',
        meta: {},
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
        name: 'Umbraco Commerce Checkout Installer Modal',
        elementName: 'ucc-installer-config-modal',
        element: () => import('../modals/installer-modal.element'),
    },
];
export const manifests = [...dashboardManifests];
