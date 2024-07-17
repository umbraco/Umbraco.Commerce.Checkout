import type { UmbModalContext } from '@umbraco-cms/backoffice/modal';
import { css, customElement, html, ifDefined, LitElement, nothing, property, state } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { ManifestModal, UmbModalExtensionElement } from '@umbraco-cms/backoffice/extension-registry';
import { UccInstallerModalSubmitValue } from './installer-modal.token';
import { UmbInputDocumentElement } from '@umbraco-cms/backoffice/document';
import type { UUIButtonState } from '@umbraco-cms/backoffice/external/uui';
import { installUmbracoCommerceCheckoutAsync } from './apis';
import { UMB_NOTIFICATION_CONTEXT, UmbNotificationContext } from '@umbraco-cms/backoffice/notification';


const ELEMENT_NAME = 'ucc-installer-config-modal';

@customElement(ELEMENT_NAME)
export default class UccInstallerConfigModal extends UmbElementMixin(LitElement)
    implements UmbModalExtensionElement<object, UccInstallerModalSubmitValue> {
    manifest?: ManifestModal | undefined;

    constructor() {
        super();
        this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
            this.#notificationContext = instance;
        });
    }

    @property({ attribute: false })
    modalContext?: UmbModalContext<object, UccInstallerModalSubmitValue>;

    #notificationContext?: UmbNotificationContext;

    @property({ attribute: false })
    data?: object;

    @state()
    private _installationRoot: string | undefined;

    @state()
    private _installButton: {
        state: UUIButtonState
    } = {
            state: undefined,
        };

    @state()
    private _cancelButton: {
        state: UUIButtonState
    } = {
            state: undefined,
        };

    @state()
    private _formState = {
        isDirty: false,
        isValid: true,
    };

    #validateForm() {
        const isValid = !!this._installationRoot;
        this._formState = {
            ...this._formState,
            isDirty: true,
            isValid,
        };

        return isValid;
    }

    #handleCancel() {
        this.modalContext?.reject();
    }

    async #handleSubmit() {
        if (!this.#validateForm()) { return; }

        this._installButton = {
            ...this._installButton,
            state: 'waiting',
        };
        this.modalContext?.updateValue({ selected: this._installationRoot });
        this.#notificationContext?.peek('default', {
            data: {
                headline: 'Umbraco Commerce Checkout',
                message: 'Installing dependencies...',
            },
        });
        try {
            const installationResult = await installUmbracoCommerceCheckoutAsync(this._installationRoot!);
            if (installationResult.success) {
                this._installButton = {
                    ...this._installButton,
                    state: 'success',
                };
                this.#notificationContext?.peek('positive', {
                    data: {
                        headline: 'Umbraco Commerce Checkout Installed',
                        message: 'Umbraco Commerce Checkout successfully installed',
                    },
                });

                this.modalContext?.submit();
            }
            else {
                this._installButton = {
                    ...this._installButton,
                    state: 'failed',
                };
                this.#notificationContext?.peek('danger', {
                    data: {
                        headline: 'Umbraco Commerce Checkout',
                        message: installationResult.message ?? 'Some errors occurred during installation process. Please try again and report to the package owner.',
                    },
                });
            }
        } catch (err) {
            this._installButton = {
                ...this._installButton,
                state: 'failed',
            };
            this.#notificationContext?.peek('danger', {
                data: {
                    headline: 'Umbraco Commerce Checkout',
                    message: JSON.stringify(err),
                },
            });
        }
    }

    #onSiteRootNodeChange(event: Event) {
        const element = event.target as UmbInputDocumentElement;
        this._installationRoot = element.value;
        this.#validateForm();
    }

    render() {
        return html`
            <umb-body-layout headline='Install Umbraco Commerce Checkout'>
                <uui-box>
                    <umb-property-layout
                        orientation='vertical'
                        ?invalid=${!this._formState.isValid}
                        label='Site Root Node'
                        description='The root node of the site under which to install the checkout pages. The node itself, or an ancestor of this node must have a fully configured store picker property defined.'
                    >
                        <umb-input-content
                            slot='editor'
                            
                            .type=${'content'}
                            .min=${1}
                            .max=${1}
                            ?showOpenButton=${false}
                            @change=${this.#onSiteRootNodeChange}
                            .value=${this._installationRoot}
                            >
                        </umb-input-content>
                    </umb-property-layout>
                    ${!this._formState.isValid
                ? html`<div class='error'>Please select the site root node</div>`
                : nothing
            }
                </uui-box>
                <umb-footer-layout slot="footer">
					<uui-button
						slot="actions"
						look="secondary"
						@click=${this.#handleCancel}
                        state=${ifDefined(this._cancelButton.state)}
						label="Cancel"></uui-button>
					<uui-button
						slot="actions"
						look="primary"
                        state=${ifDefined(this._installButton.state)}
						@click=${this.#handleSubmit}
						label="Install"></uui-button>
				</umb-footer-layout>
            </umb-body-layout>
        `;
    }

    static styles = css`
        .error {
            color: var(--uui-color-danger);
        }
    `;
}
