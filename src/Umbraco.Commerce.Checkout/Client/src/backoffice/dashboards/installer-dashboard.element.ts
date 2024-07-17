import { LitElement, css, html, customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UCC_INSTALLER_MODAL_TOKEN, UccInstallerModalSubmitValue } from './installer-modal.token';

const ELEMENT_NAME = 'uc-checkout-installer-dashboard';
@customElement(ELEMENT_NAME)
export class UcCheckoutInstallerDashboard extends UmbElementMixin(LitElement) {

  #modalManagerContext?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;

  constructor() {
    super();
    this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
      this.#modalManagerContext = instance;
      // modalManagerContext is now ready to be used.
    });
  }

  #onOpenRootPickerClick() {
    console.log('clicked');
    const modalContext = this.#modalManagerContext?.open(this, UCC_INSTALLER_MODAL_TOKEN, {
      data: {},
    });

    modalContext?.onSubmit()
      .then((res: UccInstallerModalSubmitValue) => {
        console.log(res);
      }, () => {
        console.log('User cancelled.');
      });
  }

  render() {
    return html`
     <uui-box>
        <div class="ucc-installer-wrapper">

            <!-- Header -->
            <div>
                <div style="display: inline-flex; align-items: center; justify-content: center; background-color: #141432; width: 120px; height: 120px; border-radius: 100%;">
                  <uui-icon name='icon-cash-register' style="color: white; font-size: 80px;"></uui-icon>
                </div>
            </div>
            <div class="installer-intro" style="margin-bottom: 5px;">
                <h3>Checkout</h3>
            </div>
            <p>
                Umbraco Commerce Checkout provides a ready made checkout flow for Umbraco Commerce
            </p>

            <!-- Installer -->
            <h4>Getting Started</h4>
            <p style="margin-bottom: 10px;">
              To get started with Umbraco Commerce Checkout we first need to install all the related content.<br />
              By clicking the <strong>Install</strong> button below Umbraco Commerce Checkout will install all the Data Types,<br />
              Doc Types and Content nodes needed.
            </p>
            <p>
              If you have installed Umbraco Commerce Checkout before, the installer will also perform the relevant upgrades.<br />
              <br /><strong>NB: Nothing</strong> will be removed as part of an upgrade.
            </p>
            <p style="margin-top: 30px;">
                <uui-button
                  look="primary"
                  label="Install"
                  type="button"
                  @click=${this.#onOpenRootPickerClick}></uui-button>
            </p>
        </div>
  </uui-box>
    `;
  }

  static styles = css`
    .ucc-installer-wrapper {
      margin: 20px auto 0;
      text-align: center;
      font-size: 15px;
    }

    h3 {
      font-size: 36px;
      font-weight: 700;
      letter-spacing: -1px;
      line-height: 80px;
      margin: 0 0 0 20px;
    }

    h4 {
      margin-top: 40px;
      font-weight: bold;
      font-size: 18.75px;
    }
  `;
}

export default UcCheckoutInstallerDashboard;

declare global {
  interface HTMLElementTagNameMap {
    [ELEMENT_NAME]: UcCheckoutInstallerDashboard;
  }
}