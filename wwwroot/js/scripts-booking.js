// IIFE to protect the global scope and internal variables.
(function() {

    // Pick up settings from config.js and make them usable
    var loadedpercent = 0;
    var resource = 'https://' + prefix + '-ukofficehours.azurewebsites.net';
    var endpoint = 'https://' + prefix + '-ukofficehours.azurewebsites.net/' + prefix + 'adapp';
    var rootfnsite = resource;

    // Special Binding Handlers to deal with the datepickers bindings in knockout
    ko.bindingHandlers.weekdaydatePicker = {
        init: function(element, valueAccessor, allBindingsAccessor) {

            //initialize datepicker with some optional options
            var options = allBindingsAccessor().dateTimePickerOptions || {};
            var funcOnSelecttime = function() {
                var observable = valueAccessor();
                observable($(element).datepicker("getDate"));
            }

            options.onSelect = funcOnSelecttime;

            $(element).datetimepicker({
                daysOfWeekDisabled: [0, 6],
                format: dateform
            });

            //when a user changes the date, update the view model
            ko.utils.registerEventHandler(element, "dp.change", function(event) {
                var value = valueAccessor();

                value(moment(event.date, dateform).format(dateform));
                element.blur();

            });

            ko.utils.domNodeDisposal.addDisposeCallback(element, function() {
                var picker = $(element).data("DateTimePicker");
                if (picker) {
                    picker.destroy();
                }
            });
        },
        update: function(element, valueAccessor, allBindings, viewModel, bindingContext) {

            var picker = $(element).data("DateTimePicker");

            //when the view model is updated, update the widget
            if (picker) {

                var koDate = ko.utils.unwrapObservable(valueAccessor());
                picker.date(moment(koDate, dateform));

                element.blur();

            }
        }
    };
    ko.bindingHandlers.timePicker = {
        init: function(element, valueAccessor, allBindingsAccessor) {
            //initialize datepicker with some optional options
            var options = allBindingsAccessor().dateTimePickerOptions || {};

            var funcOnSelectdate = function() {
                var observable = valueAccessor();
                observable($(element).datepicker("getDate"));
            }

            options.onSelect = funcOnSelectdate;

            $(element).datetimepicker({
                format: timeform
            });

            // when a user changes the date, update the view model
            ko.utils.registerEventHandler(element, "dp.change", function(event) {

                var value = valueAccessor();
                value(moment(event.date, timeform).format(timeform));
                element.blur();

            });

            ko.utils.domNodeDisposal.addDisposeCallback(element, function() {
                var picker = $(element).data("DateTimePicker");
                if (picker) {
                    picker.destroy();
                }
            });
        },
        update: function(element, valueAccessor, allBindings, viewModel, bindingContext) {

            var picker = $(element).data("DateTimePicker");
            //when the view model is updated, update the widget
            if (picker) {

                var koDate = ko.utils.unwrapObservable(valueAccessor());
                //$(element).datepicker("setDate", moment(koDate, timeform));
                picker.date(moment(koDate, timeform));
                $(element).blur();


            }
        }
    };

    // Viewmodels and required variables
    var isvinit = false;
    var viewmodel_addslot = {
        StartDate: ko.observable(''),
        StartTime: ko.observable(''),
        Duration: ko.observable(''),
        Break: ko.observable('')

    };

    viewmodel_addslot.StartDateTime = ko.computed(function() {
        return moment(viewmodel_addslot.StartDate() + " " + viewmodel_addslot.StartTime(), "DD/MM/YYYY HH:mm");
    });
    viewmodel_addslot.EndDateTime = ko.computed(function() {
        return moment(viewmodel_addslot.StartDate() + " " + viewmodel_addslot.StartTime(), "DD/MM/YYYY HH:mm").add(viewmodel_addslot.Duration(), "minutes");
    });

    function viewmodel_viewallslots() {

        var self = this;

        self.loadeddata = ko.observableArray(null);

        // Client side filtering code 
        // Build the lists of unique entries
        self.TEList = ko.computed(function() {
            var tes = ko.utils.arrayMap(self.loadeddata(), function(item) {
                return item.TechnicalEvangelist;
            });
            tes.push('--All--');
            return ko.utils.arrayGetDistinctValues(tes.sort()).sort();
        });
        self.PKList = ko.computed(function() {
            var tes = ko.utils.arrayMap(self.loadeddata(), function(item) {
                return item.PartitionKey;
            });
            tes.push('--All--');
            return ko.utils.arrayGetDistinctValues(tes.sort()).sort();
        });
        self.ISVList = ko.computed(function() {
            var list = ko.utils.arrayMap(self.loadeddata(), function(item) {
                return item.BookedToISV;
            });
            list.push('--All--');
            return ko.utils.arrayGetDistinctValues(list.sort()).sort();
        });
        self.PBEList = ko.computed(function() {
            var list = ko.utils.arrayMap(self.loadeddata(), function(item) {
                return item.PBE;
            });
            list.push('--All--');
            return ko.utils.arrayGetDistinctValues(list.sort()).sort();
        });
        self.DateList = ko.computed(function() {
            var list = ko.utils.arrayMap(self.loadeddata(), function(item) {
                return moment(item.StartDateTime).format('dddd, MMMM Do YYYY');
            });
            list.push('--All--');
            return ko.utils.arrayGetDistinctValues(list);
        });
        self.DurationList = ko.computed(function() {
            var list = ko.utils.arrayMap(self.loadeddata(), function(item) {
                return item.Duration;
            });
            list.push('--All--');
            return ko.utils.arrayGetDistinctValues(list.sort()).sort();
        });
        self.FutureOrPastList = ['--All--', 'Future', 'Past'];

        // Bind the filter values themselves
        self.FilterTE = ko.observable(localStorage.getItem("FilterTE") || '--All--');
        self.FilterPK = ko.observable(localStorage.getItem("FilterPK") || '--All--');
        self.FilterPBE = ko.observable(localStorage.getItem("FilterPBE") || '--All--');
        self.FilterISV = ko.observable(localStorage.getItem("FilterISV") || '--All--');
        self.FilterDate = ko.observable(localStorage.getItem("FilterDate") || '--All--');
        self.FilterDuration = ko.observable(localStorage.getItem("FilterDuration") || '--All--');
        self.FilterFutureOrPast = ko.observable(localStorage.getItem("FilterFutureOrPast") || '--All--');

        self.ResetFilters = function() {

            // Bind the filter values themselves
            self.FilterTE('--All--');
            self.FilterPK('--All--');
            self.FilterPBE('--All--');
            self.FilterISV('--All--');
            self.FilterDate('--All--');
            self.FilterDuration('--All--');
            self.FilterFutureOrPast('--All--');


        };
        self.SaveFilters = function() {

            // Store the filter values IN html5 LOCALSTORAGE
            localStorage.setItem("FilterTE", self.FilterTE());
            localStorage.setItem("FilterPK", self.FilterPK());
            localStorage.setItem("FilterPBE", self.FilterPBE());
            localStorage.setItem("FilterISV", self.FilterISV());
            localStorage.setItem("FilterDate", self.FilterDate());
            localStorage.setItem("FilterDuration", self.FilterDuration());
            localStorage.setItem("FilterFutureOrPast", self.FilterFutureOrPast());

        };
        self.LoadFilters = function() {

            // Fetch the filters and apply them
            self.FilterFutureOrPast(localStorage.getItem("FilterFutureOrPast") || '--All--');
            self.FilterDuration(localStorage.getItem("FilterDuration") || '--All--');
            self.FilterISV(localStorage.getItem("FilterISV") || '--All--');
            self.FilterTE(localStorage.getItem("FilterTE") || '--All--');
            self.FilterPK(localStorage.getItem("FilterPK") || '--All--');
            self.FilterPBE(localStorage.getItem("FilterPBE") || '--All--');
            self.FilterDate(localStorage.getItem("FilterDate") || '--All--');

            $("#notify-settings").html(" Filters Loaded");

        };

        // Get the main page content if we are not on the first page and render the stuff to the DOM
        authContext.acquireToken(clientid, function(error, token) {

            $.ajax({

                method: "GET",
                url: rootfnsite + "api/GetAllBookingSlots",
                headers: {
                    'authorization': 'bearer ' + token
                },

                success: function(result) {
                    self.loadeddata(result);
                    self.LoadFilters();

                }
            });

        });

        // Helper Method to do the filter and include the --All-- value as true
        self.SearchForMatchInArrayAgainstItemHelper = function(filterSelection, filteredItem) {

            if (filterSelection === '--All--') { return true; } else { return filterSelection === filteredItem; }
        };
        // Helper Method to filter forward or back depending on value passed in
        self.SearchForMatchInArrayAgainstDateHelper = function(filterSelection, filteredItem) {

            switch (filterSelection) {

                case '--All--':

                    return true;


                case 'Future':

                    return moment().diff(filteredItem, 'minutes') < 0;


                case 'Past':

                    return moment().diff(filteredItem, 'minutes') >= 0;


            }

        };

        // Calculate which rows to show across all filters
        self.filteredItems = ko.computed(function() {

            return ko.utils.arrayFilter(self.loadeddata(), function(row) {

                var TEFilterPass = self.SearchForMatchInArrayAgainstItemHelper(self.FilterTE(), row.TechnicalEvangelist);
                var DateFilterPass = self.SearchForMatchInArrayAgainstItemHelper(self.FilterDate(), moment(row.StartDateTime).format('dddd, MMMM Do YYYY'));
                var DurationFilterPass = self.SearchForMatchInArrayAgainstItemHelper(self.FilterDuration(), row.Duration);
                var ISVFilterPass = self.SearchForMatchInArrayAgainstItemHelper(self.FilterISV(), row.BookedToISV);
                var FutureOrPastPass = self.SearchForMatchInArrayAgainstDateHelper(self.FilterFutureOrPast(), row.StartDateTime);
                var PKPass = self.SearchForMatchInArrayAgainstItemHelper(self.FilterPK(), row.PartitionKey);
                var PBEPass = self.SearchForMatchInArrayAgainstItemHelper(self.FilterPBE(), row.PBE);
                return TEFilterPass && DateFilterPass && DurationFilterPass && ISVFilterPass && FutureOrPastPass && PKPass && PBEPass;

            });
        });

        ko.applyBindings(self, document.getElementById("bindingforBookedlist"));
    }

    function viewmodel_viewallisvs() {

        var self = this;

        self.loadeddata = ko.observableArray(null);

        loadstarted();

        // Client side filtering code 
        // Build the lists of unique entries
        //self.TEList = ko.computed(function () {
        //    var tes = ko.utils.arrayMap(self.loadeddata(), function (item) {
        //        return item.TechnicalEvangelist;
        //    });
        //    tes.push('--All--');
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

    }

    var viewmodel_viewslots = {

        loadeddata: null,
        submitdata: {

            RowKey: ko.observable('ERROR'),
            PartitionKey: ko.observable('ERROR'),
            BookingCode: ko.observable($.QueryString.BookingCode),
            VisualSlot: ko.observable('2017-02-12T17:00:00.000')

        },
        FireBooking: function() {

            authContext.acquireToken(clientid, function(error, token) {

                loadupdatestatus(15);

                self = viewmodel_viewslots;
                //viewmodel_viewslots.submitdata.BookingCode();

                $('#sendbobtn').prop("disabled", true);
                loadupdatestatus(30);
                $.ajax({

                    method: "POST",
                    contentType: "application/json",
                    url: rootfnsite + "api/UpdateBookingSlot-BookToISV",
                    data: ko.toJSON(self.submitdata),
                    headers: {
                        'authorization': 'bearer ' + token
                    },
                    success: function(result) {

                        loadupdatestatus(70);

                        self.submitdata.BookingCode('');

                        $('#statusbosend').html("OK, You're booked in!");
                        document.getElementById("statusbosend").className += "bg-success";
                        $('#sendbobtn').prop("disabled", false);
                        loadfinished();

                    },

                    error: function(result) {

                        $('#statusbosend').html("Failed: " + result.responseText);
                        document.getElementById("statusbosend").className += "bg-danger";
                        loadfinished();

                    }

                });

            });
        },
        SetBookingSlot: function(bookingslot) {


            // Set up the modal form data bindings to receive the selected row values
            viewmodel_viewslots.submitdata.RowKey(bookingslot.RowKey);
            viewmodel_viewslots.submitdata.PartitionKey(bookingslot.PartitionKey);
            viewmodel_viewslots.submitdata.VisualSlot(moment(bookingslot.StartDateTime).format("ddd, DD MMM YYYY @ HH:mm") + " for " + bookingslot.Duration + " Mins");

            $('#sendbobtn').prop("disabled", false);

            // Exit and allow bootstrap to pop up the modal
            return true;
        }

    };

    var viewmodel_isvdata = {
        Name: ko.observable(''),
        ContactEmail: ko.observable(''),
        ContactName: ko.observable(''),
        CurrentCode: ko.observable(''),
        EmailLink: ko.observable('')
    };


    function loadstarted() {

        loadedpercent = 0;
        $("#progbartop").toggle(true);

    }

    function loadfinished() {

        loadedpercent = 100;
        $("#progbartop").toggle(false);

    }

    function loadupdatestatus(addpercent) {

        $("#progbartop").toggle(true);
        loadedpercent = loadedpercent + addpercent;
        $('#progbar').width(loadedpercent + '%');

    }

    var allbookingslotsbound = false;
    var allisvsbound = false;
    var myisvsforfilter;

    // Ajax Binding functions
    function fetchAllBookingSlotsViaFunction() {

        if (allbookingslotsbound === false) {

            loadstarted();

            myslotsforfilter = new viewmodel_viewallslots();

            allbookingslotsbound = true;

            myslotsforfilter.LoadFilters();

            loadfinished();

        }

    }

    // Ajax Binding functions
    function fetchAllisvsViaFunction() {

        if (allisvsbound === false) {

            loadstarted();

            //myisvsforfilter = new viewmodel_viewallisvs();

            //allisvsbound = true;

            //myisvsforfilter.LoadFilters();

            loadfinished();

        }


    }

    function fetchBookingSlotsViaFunction() {

        if (viewmodel_viewslots.loadeddata === null) {

            loadstarted();

            // Get the main page content if we are not on the first page and render the stuff to the DOM
            authContext.acquireToken(clientid, function(error, token) {

                $.getJSON(rootfnsite + "api/GetBookingSlot", function(data) {

                    viewmodel_viewslots.loadeddata = data;

                    loadupdatestatus(50);

                    ko.applyBindings(viewmodel_viewslots, document.getElementById("bindingforBookinglist"));
                    ko.applyBindings(viewmodel_viewslots, document.getElementById("myBookingModal"));

                    loadfinished();

                });
            });

        } else {

            loadstarted();

            authContext.acquireToken(clientid, function(error, token) {

                $.getJSON(rootfnsite + "api/GetBookingSlot", function(data) {

                    viewmodel_viewslots.loadeddata = data;

                    loadupdatestatus(50);

                    loadfinished();

                });

            });

        }

    }

    function wireSlotSubmitFormViaFunction() {

        // loadstarted();

        viewmodel_addslot.WriteBookingSlot = function(data) {

            self = this;
            var sender = ko.toJSON(self);

            authContext.acquireToken(clientid, function(error, token) {

                loadupdatestatus(30);

                $('#sendslotbtn').prop("disabled", true);

                $.ajax({

                    method: "POST",
                    contentType: "application/json",
                    url: rootfnsite + "api/InsertBookingSlot",
                    data: sender,
                    headers: {
                        'authorization': 'bearer ' + token
                    },
                    success: function(result) {

                        loadupdatestatus(60);

                        var currentStartDateTime = moment(viewmodel_addslot.StartDateTime());
                        var nextSlotStartOffset = parseInt(viewmodel_addslot.Duration()) + parseInt(viewmodel_addslot.Break());
                        var nextStartDateTimeMoment = currentStartDateTime.add(nextSlotStartOffset, 'minutes');
                        var nextStartDateTime = nextStartDateTimeMoment.format('HH:mm');

                        $('#statussendbook').html("Slot Added @ " + viewmodel_addslot.StartTime() + " of duration " + viewmodel_addslot.Duration() + " mins and next slot ready for " + viewmodel_addslot.Break() + " mins post completion.");

                        viewmodel_addslot.StartTime(nextStartDateTime);
                        //loadfinished();
                        $('#sendslotbtn').prop("disabled", false);

                    },

                    error: function(result) {

                        $('#statussendbook').html("Send Failed");
                        //loadfinished();
                        $('#sendslotbtn').prop("disabled", false);
                    }

                });
            });

        };

        if (isvinit === false) {

            ko.applyBindings(viewmodel_addslot, document.getElementById("bookingslotForm"));
            isvinit = true;
        }

    }

    function wireISVSubmitFormViaFunction() {

        //loadstarted();

        viewmodel_isvdata.WriteISV = function(data) {

            self = this;
            var sender = ko.toJSON(self);

            authContext.acquireToken(clientid, function(error, token) {

                //loadupdatestatus(30);

                $('#sendisvbtn').prop("disabled", true);

                $.ajax({

                    method: "POST",
                    contentType: "application/json",
                    url: rootfnsite + "api/InsertISV",
                    data: sender,
                    headers: {
                        'authorization': 'bearer ' + token
                    },
                    success: function(result) {

                        loadupdatestatus(60);

                        // resetisvdata();

                        // Success: Clear down old values

                        var subject = "subject=Your Free Azure VIP Office Hours Session Code";
                        var cc = "cc=" + result.PartitionKey;
                        var uri = encodeURI("http://azurevip.azurewebsites.net/booking.html?StartPanel=bookwithcode&BookingCode=" + result.CurrentCode);
                        var body = "body=Dear " + result.ContactName + ", \r\n \r\nYour Exclusive Azure VIP Code is: " + result.CurrentCode + "\r\nTo book your VIP Office Hours session with a Microsoft Azure Technical Specialist then Head to " + uri;
                        var link = 'mailto:' + result.ContactEmail;
                        var concatlink = link + "&" + cc + "&" + subject + "&" + body;


                        viewmodel_isvdata.EmailLink = ko.observable('');
                        viewmodel_isvdata.ContactEmail('');
                        viewmodel_isvdata.ContactName('');
                        viewmodel_isvdata.Name('');
                        viewmodel_isvdata.CurrentCode('');

                        $('#sendlink').attr('href', uri);
                        $('#sendlink').prop("disabled", false);
                        $('#statussend').html(body);

                        //loadfinished();
                        $('#sendisvbtn').prop("disabled", false);


                    },

                    error: function(result) {

                        $('#statussend').html("Failed: " + result.responseText);
                        //loadfinished();
                        $('#sendisvbtn').prop("disabled", false);
                    }

                });
            });

        };

        if (isvinit === false) {

            ko.applyBindings(viewmodel_isvdata, document.getElementById("isvForm"));
            isvinit = true;
        }

    }

    // Render-on-demand functions, show and hide DOM elements for SPA items.
    function showpanel(panelOn) {

        // Switch off a panel then turn on the new panel.
        $("#" + visiblepanel + "section").toggle();
        $("#" + panelOn + "section").toggle();

        visiblepanel = panelOn;

        initpanel(visiblepanel);

    }

    function initpanel(panelOn) {

        // Initialise the appropriate panel loader here on demand

        switch (panelOn) {

            case "viewslots":

                fetchAllBookingSlotsViaFunction();

                break;

            case "viewisvs":

                fetchAllisvsViaFunction();

                break;

            case "bookwithcode":

                fetchBookingSlotsViaFunction();
                break;

            case "addslots":

                wireSlotSubmitFormViaFunction();
                break;

            case "addisv":

                wireISVSubmitFormViaFunction();
                break;
        }
    }

    $("#progbartop").toggle();

    // Startup code
    loadstarted();

    // Hide the main panels from display as we won't bind all of them immediately until they are needed.
    $("#viewslotssection").toggle();
    $("#viewisvssection").toggle();
    $("#addslotssection").toggle();
    $("#addisvsection").toggle();
    $("#bookwithcodesection").toggle();
    var visiblepanel = "welcome";

    loadupdatestatus(30);

    // Now add the click handlers to hide and show the relevant panels calling the functions above.
    document.getElementById("viewslotsbutton").onclick = function() { showpanel("viewslots"); };
    document.getElementById("viewisvsbutton").onclick = function() { showpanel("viewisvs"); };
    document.getElementById("addslotsbutton").onclick = function() { showpanel("addslots"); };
    document.getElementById("addisvbutton").onclick = function() { showpanel("addisv"); };
    document.getElementById("bookwithcodebutton").onclick = function() { showpanel("bookwithcode"); };

    // ClientID etc are declared and present at row 281 

    // AuthN stuff for the Security Demo using adal.js
    var authContext = new AuthenticationContext({
        instance: 'https://login.microsoftonline.com/',
        tenant: tenantid,
        clientId: clientid,
        postLogoutRedirectUri: window.location.origin,
        cacheLocation: 'localStorage'
    });

    // Check For & Handle Redirect From AAD After Login 
    var isCallback = authContext.isCallback(window.location.hash);
    authContext.handleWindowCallback();

    loadupdatestatus(30);

    var log = document.getElementById("loginorout");

    // If already logged in then show logout box.
    if (!authContext.getCachedUser()) {

        document.getElementById("loginorout").className += " btn btn-alert";

        log.html = "logmein";
        log.onclick = function() {

            authContext.config.redirectUri = window.location.href;
            authContext.login();

        };
        $("#menuleft").toggle();
    } else {

        // Show the logged in userID on the title bar 

        document.getElementById("loginorout").className += " btn btn-success";

        log.innerHTML = "Logout " + authContext.getCachedUser().profile.name;

        log.onclick = function() { authContext.logOut(); };

    }

    loadfinished();

    authContext.acquireToken('https://graph.microsoft.com', function(error, token) {

        $.ajax({

            method: "GET",
            url: "https://graph.microsoft.com/v1.0/me/events?$top=5",
            headers: {
                'authorization': 'bearer ' + token
            },
            success: function(result) {

                document.getElementById("GRAPHDATA").html = result;

            }
        });

    });

    // auto-switch to the requested panel on ready()
    $(document).ready(function() {
        showpanel($.QueryString.StartPanel);

    });

    // Enable the Bootstrap Datetimepicker controls with sat and sun disabled, in uk dd/mm/yyyy date format 
    $(function() {
        $('#StartDate').datetimepicker({
            daysOfWeekDisabled: [0, 6],
            format: 'DD/MM/YYYY'
        });
    });

    $(function() {
        $('#StartTime').datetimepicker({
            format: 'HH:mm'
        });
    });

})();