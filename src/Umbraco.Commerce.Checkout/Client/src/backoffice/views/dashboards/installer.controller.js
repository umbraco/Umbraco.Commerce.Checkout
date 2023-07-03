(function() {

    'use strict';

    function InstallerDashboardController($scope, $routeParams, editorService) {

        var vm = this;

        var dialogOptions = {
            view: '/App_Plugins/UmbracoCommerceCheckout/backoffice/views/dialogs/installer.html',
            size: 'small',
            config: { },
            submit: function (model) {
                editorService.close();
            },
            close: function () {
                editorService.close();
            }
        };

        vm.openInstaller = function() {
            editorService.open(dialogOptions);
        };

    }

    angular.module('umbraco.commerce').controller('Umbraco.Commerce.Checkout.Controllers.InstallerDashboardController', InstallerDashboardController);

}());
