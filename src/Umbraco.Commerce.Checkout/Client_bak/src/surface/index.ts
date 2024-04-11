
import './css/main.css';

// Initialization
function init() {

    // Setup order summary toggle
    document.getElementById('order-summary-toggle')?.addEventListener('click', function (e) {
        e.preventDefault();
        const osEl = document.getElementById('order-summary');
        const showOrderSummary = osEl?.classList.contains('hidden');
        osEl?.classList.toggle('hidden', !showOrderSummary);
        document.getElementById('order-summary-toggle__text-open')?.classList.toggle('hidden', !showOrderSummary);
        document.getElementById('order-summary-toggle__text-closed')?.classList.toggle('hidden', showOrderSummary);
    });

    // Display billing address regions if any
    document.querySelectorAll('select[name=\'billingAddress.Country\']').forEach(el => {
        const selectEl = el as HTMLSelectElement;
        const h = () => { toggleShippingRequiredInputValidation(); showRegions('billing', JSON.parse(selectEl.selectedOptions[0].dataset.regions || '')); };
        el.addEventListener('change', h);
        h();
    });

    // Display shipping address regions if any
    document.querySelectorAll('select[name=\'shippingAddress.Country\']').forEach(el => {
        const selectEl = el as HTMLSelectElement;
        const h = () => { toggleShippingRequiredInputValidation(); showRegions('shipping', JSON.parse(selectEl.selectedOptions[0].dataset.regions || '')); };

        el.addEventListener('change', h);
        h();
    });

    // Toggle shipping address display
    document.querySelectorAll('input[name=shippingSameAsBilling]').forEach(el => {
        const inputEl = el as HTMLInputElement;
        const h = () => { document.getElementById('shipping-info')?.classList.toggle('hidden', inputEl.checked); toggleShippingRequiredInputValidation(); };

        el.addEventListener('change', h);
        h();
    });

    // Update shippingOptionId based on selected shipping method
    document.querySelectorAll('input[name=shippingMethod]').forEach(el => {
        const h = () => {
            const shippingOptionEl: HTMLInputElement | null = document.querySelector('input[name=shippingOptionId]');
            if (!shippingOptionEl) {
                return;
            }

            shippingOptionEl.value = (<HTMLElement | null>document.querySelector('input[name=shippingMethod]:checked'))?.dataset.optionId || '';
        };

        el.addEventListener('change', h);
        if ((<HTMLInputElement>el).checked) h();
    });

    // Enable / disable continue button when accepting terms
    const acceptTermsEl = document.getElementById('accept-terms') as HTMLInputElement;
    const continueBtn = document.getElementById('continue') as HTMLButtonElement;
    if (acceptTermsEl) {
        acceptTermsEl.addEventListener('change', () => {
            continueBtn.disabled = !acceptTermsEl.checked;
        });
    } else {
        continueBtn.disabled = false;
    }
}

// Helper functions
function toggleShippingRequiredInputValidation() {
    const shippingSameAsBillingEl = document.querySelector('input[name=shippingSameAsBilling]') as HTMLInputElement;
    const shippingSameAsBilling = shippingSameAsBillingEl && shippingSameAsBillingEl.checked;
    document.querySelectorAll('#shipping-info [required]').forEach(el => {
        const htmlEl = el as HTMLInputElement;
        htmlEl.disabled = shippingSameAsBilling; // Disable any shipping required fields (this overrides the required validation)
    });
}

type SelectItem = {
    id: string,
    name: string,
}

function showRegions(addressType: string, regions: SelectItem[]) {
    const sl = document.querySelector('select[name=\'' + addressType + 'Address.Region\']') as HTMLSelectElement;
    const slVal = sl.dataset.value || '';

    sl.innerHTML = '';

    const hasRegions = regions.length > 0;
    if (hasRegions) {
        let containsValue = false;

        const opt = document.createElement('option');
        opt.value = '';
        opt.text = sl.dataset.placeholder || '';
        opt.disabled = true;
        opt.selected = true;
        sl.appendChild(opt);

        regions.forEach(function (itm) {
            const opt = document.createElement('option');
            opt.value = itm.id;
            opt.text = itm.name;
            sl.appendChild(opt);
            if (slVal && itm.id === slVal) {
                containsValue = true;
            }
        });

        if (containsValue) {
            sl.value = slVal;
        }
    }

    sl.required = hasRegions;
    sl.disabled = !hasRegions;
};

// Trigger init
init();



