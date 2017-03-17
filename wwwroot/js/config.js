// Settings for app to be loaded into the global scope
// DEV Configuration
// We could make this server agnostic by declaring these server-side but this is a js spa so we will simply switch the config file out at deploy time

var prefix = 'cilocal' // Dummy CI Deployment prefix 
var clientid = ''; // Dummy CI Deployment Azure AD App 

var instance = 'https://login.microsoftonline.com/'; // Azure AD logon endpoint

var tenantid = 'microsoft.com'; // Azure AD Tenant
var dateform = "DD/MM/YYYY"; // UK locale
var timeform = "HH:mm"; // 12h UK time

// Pick up server specific settings declared in config.js and make them usable in this script
var resource = 'https://' + prefix + '-ukofficehours.azurewebsites.net';
var endpoint = 'https://' + prefix + '-ukofficehours.azurewebsites.net/';
var rootfnsite = resource;
//    return ko.utils.arrayGetDistinctValues(tes.sort()).sort();
//});
//self.PKList = ko.computed(function () {
//    var tes = ko.utils.arrayMap(self.loadeddata(), function (item) {
//        return item.PartitionKey;
//    });
//    tes.push('--All--');
//    return ko.utils.arrayGetDistinctValues(tes.sort()).sort();
//});
//self.ISVList = ko.computed(function () {
//    var list = ko.utils.arrayMap(self.loadeddata(), function (item) {
//        return item.BookedToISV;
//    });
//    list.push('--All--');
//    return ko.utils.arrayGetDistinctValues(list.sort()).sort();
//});
//self.PBEList = ko.computed(function () {
//    var list = ko.utils.arrayMap(self.loadeddata(), function (item) {
//        return item.PBE;
//    });
//    list.push('--All--');
//    return ko.utils.arrayGetDistinctValues(list.sort()).sort();
//});
//self.DateList = ko.computed(function () {
//    var list = ko.utils.arrayMap(self.loadeddata(), function (item) {
//        return moment(item.StartDateTime).format('dddd, MMMM Do YYYY');
//    });
//    list.push('--All--');
//    return ko.utils.arrayGetDistinctValues(list);
//});
//self.DurationList = ko.computed(function () {
//    var list = ko.utils.arrayMap(self.loadeddata(), function (item) {
//        return item.Duration;
//    });
//    list.push('--All--');
//    return ko.utils.arrayGetDistinctValues(list.sort()).sort();
//});
//self.FutureOrPastList = ['--All--', 'Future', 'Past'];

//// Bind the filter values themselves
//self.FilterTE = ko.observable(localStorage.getItem("FilterTE") || '--All--');
//self.FilterPK = ko.observable(localStorage.getItem("FilterPK") || '--All--');
//self.FilterPBE = ko.observable(localStorage.getItem("FilterPBE") || '--All--');
//self.FilterISV = ko.observable(localStorage.getItem("FilterISV") || '--All--');
//self.FilterDate = ko.observable(localStorage.getItem("FilterDate") || '--All--');
//self.FilterDuration = ko.observable(localStorage.getItem("FilterDuration") || '--All--');
//self.FilterFutureOrPast = ko.observable(localStorage.getItem("FilterFutureOrPast") || '--All--');

//self.ResetFilters = function () {

//    // Bind the filter values themselves
//    self.FilterTE('--All--');
//    self.FilterPK('--All--');
//    self.FilterPBE('--All--');
//    self.FilterISV('--All--');
//    self.FilterDate('--All--');
//    self.FilterDuration('--All--');
//    self.FilterFutureOrPast('--All--');


//};
//self.SaveFilters = function () {

//    // Store the filter values IN html5 LOCALSTORAGE
//    localStorage.setItem("FilterTE", self.FilterTE());
//    localStorage.setItem("FilterPK", self.FilterPK());
//    localStorage.setItem("FilterPBE", self.FilterPBE());
//    localStorage.setItem("FilterISV", self.FilterISV());
//    localStorage.setItem("FilterDate", self.FilterDate());
//    localStorage.setItem("FilterDuration", self.FilterDuration());
//    localStorage.setItem("FilterFutureOrPast", self.FilterFutureOrPast());

//};
//self.LoadFilters = function () {

//    // Fetch the filters and apply them
//    self.FilterFutureOrPast(localStorage.getItem("FilterFutureOrPast") || '--All--');
//    self.FilterDuration(localStorage.getItem("FilterDuration") || '--All--');
//    self.FilterISV(localStorage.getItem("FilterISV") || '--All--');
//    self.FilterTE(localStorage.getItem("FilterTE") || '--All--');
//    self.FilterPK(localStorage.getItem("FilterPK") || '--All--');
//    self.FilterPBE(localStorage.getItem("FilterPBE") || '--All--');
//    self.FilterDate(localStorage.getItem("FilterDate") || '--All--');

//    $("#notify-settings").html(" Filters Loaded");

//};

//// Get the main page content if we are not on the first page and render the stuff to the DOM
//authContext.acquireToken(clientid, function (error, token) {

//    $.ajax({

//        method: "GET",
//        url: rootfnsite + "api/GetAllBookingSlots",
//        headers: {
//            'authorization': 'bearer ' + token
//        },

//        success: function (result) {
//            self.loadeddata(result);
//            self.LoadFilters();

//        }
//    });

//});

//// Helper Method to do the filter and include the --All-- value as true
//self.SearchForMatchInArrayAgainstItemHelper = function (filterSelection, filteredItem) {

//    if (filterSelection === '--All--') { return true; }
//    else { return filterSelection === filteredItem; }
//};
//// Helper Method to filter forward or back depending on value passed in
//self.SearchForMatchInArrayAgainstDateHelper = function (filterSelection, filteredItem) {

//    switch (filterSelection) {

//        case '--All--':

//            return true;


//        case 'Future':

//            return moment().diff(filteredItem, 'minutes') < 0;


//        case 'Past':

//            return moment().diff(filteredItem, 'minutes') >= 0;


//    }

//};

//// Calculate which rows to show across all filters
//self.filteredItems = ko.computed(function () {

//    return ko.utils.arrayFilter(self.loadeddata(), function (row) {

//        var TEFilterPass = self.SearchForMatchInArrayAgainstItemHelper(self.FilterTE(), row.TechnicalEvangelist);
//        var DateFilterPass = self.SearchForMatchInArrayAgainstItemHelper(self.FilterDate(), moment(row.StartDateTime).format('dddd, MMMM Do YYYY'));
//        var DurationFilterPass = self.SearchForMatchInArrayAgainstItemHelper(self.FilterDuration(), row.Duration);
//        var ISVFilterPass = self.SearchForMatchInArrayAgainstItemHelper(self.FilterISV(), row.BookedToISV);
//        var FutureOrPastPass = self.SearchForMatchInArrayAgainstDateHelper(self.FilterFutureOrPast(), row.StartDateTime);
//        var PKPass = self.SearchForMatchInArrayAgainstItemHelper(self.FilterPK(), row.PartitionKey);
//        var PBEPass = self.SearchForMatchInArrayAgainstItemHelper(self.FilterPBE(), row.PBE);
//        return TEFilterPass && DateFilterPass && DurationFilterPass && ISVFilterPass && FutureOrPastPass && PKPass && PBEPass;

//    });
//});

//loadupdatestatus(50);

//ko.applyBindings(self, document.getElementById("bindingforBookedlist"));

//loadfinished();

// }

// // Ajax Binding functions
// function fetchAllisvsViaFunction() {
//     if (allisvsbound === false) {
//         loadstarted();
//         //myisvsforfilter = new viewmodel_viewallisvs();
//         //allisvsbound = true;
//         //myisvsforfilter.LoadFilters();
//         loadfinished();
//     }

// }// This code is to get a token to hit the MS Graph endpoint (to enquire on a user's calendar at some point).
// ToDo: Currently dormant
// authContext.acquireToken('https://graph.microsoft.com', function(error, token) {
//     $.ajax({
//         method: "GET",
//         url: "https://graph.microsoft.com/v1.0/me/events?$top=5",
//         headers: {
//             'authorization': 'bearer ' + token
//         },
//         success: function(result) {
//             document.getElementById("GRAPHDATA").html = result;
//         }
//     });
// });