(function() {

    'use strict';

    function ucCheckoutResource($http, umbRequestHelper) {

        return {

          installUmbracoCommerceCheckout: function(siteRootNodeId) {
                return umbRequestHelper.resourcePromise(
                  $http.get("/umbraco/backoffice/UmbracoCommerceCheckout/UmbracoCommerceCheckoutApi/InstallUmbracoCommerceCheckout?siteRootNodeId=" + siteRootNodeId),
                    "Failed to install Umbraco Commerce Checkout");
            }

        };

    };

  angular.module('umbraco.commerce.resources').factory('ucCheckoutResource', ucCheckoutResource);

}());
