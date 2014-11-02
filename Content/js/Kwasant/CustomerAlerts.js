﻿var pollTimeInSeconds = 5;

function getUpdateForPage(eventName, objectID, callback) {
    $.post(
        '/Alerting/RegisterInterestInPageUpdates',
        {
            eventName: eventName,
            objectID: objectID
        },
        function (guid) {

            setInterval(function () {
                $.post(
                    '/Alerting/RequestUpdate',
                    { guid: guid },
                    function (result) {
                        if(callback)
                            callback(result);
                    }
                );
            }, pollTimeInSeconds * 1000);
        }
    );
}

function getUpdateForUser(eventName, callback) {
    $.post(
        '/Alerting/RegisterInterestInUserUpdates',
        {
            eventName: eventName,
        },
        function(guid) {

            setInterval(function() {
                $.post(
                    '/Alerting/RequestUpdateForUser',
                    { guid: guid },
                    function (result) {
                        if (callback)
                            callback(result);
                    }
                );
            }, pollTimeInSeconds * 1000);
        }
    );
}

function displayBottomRight(message, onClick, onClose, removeButtonClassName) {
    if (!removeButtonClassName)
        removeButtonClassName = "glyphicon glyphicon-remove";
    
    $.notify.addStyle('newbookingrequest', {
        html: "<div>" +
            "<div class='clearfix'>" +
            "<div class='customNotification alert alert-warning'>" +
            "<span class='glyphicon glyphicon-exclamation-sign'></span>  " + message +
            "</div>" +
            "</div>" +
            "</div>"
    });

    $(document).on('click', '.notifyjs-newbookingrequest-base', function (ev) {
        if (ev.target.className == removeButtonClassName) {
            $(this).trigger('notify-hide');
            if (onClose)
                onClose();
        } else {
            if (onClick)
                onClick();
        }
    });

    $.notify('Message', {
        style: 'newbookingrequest',
        className: 'superblue',
        autoHide: false,
        clickToHide: false,
        globalPosition: 'bottom right'
    });
}

if (NewBookingRequestForUserQueueListenerEnabled) {
    getUpdateForUser('NewBookingRequestForUserQueue', function (result) {
        $.each(result, function(ind, row) {
            var message = 'A new booking request has been assigned to you!  <span style="margin-left:5px" class="glyphicon glyphicon-remove"></span></br> Click here to take a look.';

            displayBottomRight(message, function() {
                location.href = '/Dashboard/Index?id=' + row.BookingRequestID;
            });
        });
    });
}