(function() {

    'use strict';

    function InstallerDialogController($scope, $routeParams, formHelper, notificationsService, ucCheckoutResource) {

        var vm = this;

        vm.loading = true;
        vm.buttonStates = {
            cancel: 'init',
            install: 'init'
        };

        vm.property = {
            alias: 'siteRootNode',
            label: 'Site Root Node',
            description: 'The root node of the site under which to install the checkout pages. The node itself, or an ancestor of this node must have a fully configured store picker property defined.',
            validation: {
                mandatory: true
            },
            view: 'contentpicker',
            value: null,
            config: {
                startNode: -1,
                idType: 'udi'
            }
        };

        vm.cancel = function() {
            $scope.model.close();
        };

        vm.install = function() {
            if (formHelper.submitForm({ scope: $scope })) {
                vm.buttonStates.install = 'busy';
                ucCheckoutResource.installUmbracoCommerceCheckout(vm.property.value).then(function(result) {
                    if (result.success) {
                        notificationsService.success("Umbraco Commerce Checkout Installed", "Umbraco Commerce Checkout successfully installed");
                        $scope.model.submit();
                    } else {
                        notificationsService.error(result.message);
                        vm.buttonStates.install = 'error';
                    }
                });
            }
        };

        vm.init = function() {
            vm.loading = false;
        };

        vm.init();
    }

    angular.module('umbraco.commerce').controller('Umbraco.Commerce.Checkout.Controllers.InstallerDialogController', InstallerDialogController);

}());
