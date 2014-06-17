/**
* Cookie plugin
*
* Copyright (c) 2006 Klaus Hartl (stilbuero.de)
* Dual licensed under the MIT and GPL licenses:
* http://www.opensource.org/licenses/mit-license.php
* http://www.gnu.org/licenses/gpl.html
*
*/

/**
* Create a cookie with the given name and value and other optional parameters.
*
* @example $.cookie('the_cookie', 'the_value');
* @desc Set the value of a cookie.
* @example $.cookie('the_cookie', 'the_value', { expires: 7, path: '/', domain: 'jquery.com', secure: true });
* @desc Create a cookie with all available options.
* @example $.cookie('the_cookie', 'the_value');
* @desc Create a session cookie.
* @example $.cookie('the_cookie', null);
* @desc Delete a cookie by passing null as value. Keep in mind that you have to use the same path and domain
*       used when the cookie was set.
*
* @param String name The name of the cookie.
* @param String value The value of the cookie.
* @param Object options An object literal containing key/value pairs to provide optional cookie attributes.
* @option Number|Date expires Either an integer specifying the expiration date from now on in days or a Date object.
*                             If a negative value is specified (e.g. a date in the past), the cookie will be deleted.
*                             If set to null or omitted, the cookie will be a session cookie and will not be retained
*                             when the the browser exits.
* @option String path The value of the path atribute of the cookie (default: path of page that created the cookie).
* @option String domain The value of the domain attribute of the cookie (default: domain of page that created the cookie).
* @option Boolean secure If true, the secure attribute of the cookie will be set and the cookie transmission will
*                        require a secure protocol (like HTTPS).
* @type undefined
*
* @name $.cookie
* @cat Plugins/Cookie
* @author Klaus Hartl/klaus.hartl@stilbuero.de
*/

/**
* Get the value of a cookie with the given name.
*
* @example $.cookie('the_cookie');
* @desc Get the value of a cookie.
*
* @param String name The name of the cookie.
* @return The value of the cookie.
* @type String
*
* @name $.cookie
* @cat Plugins/Cookie
* @author Klaus Hartl/klaus.hartl@stilbuero.de
*/
jQuery.cookie = function (name, value, options) {
    if (typeof value != 'undefined') { // name and value given, set cookie
        options = options || {};
        if (value === null) {
            value = '';
            options.expires = -1;
        }
        var expires = '';
        if (options.expires && (typeof options.expires == 'number' || options.expires.toUTCString)) {
            var date;
            if (typeof options.expires == 'number') {
                date = new Date();
                date.setTime(date.getTime() + (options.expires * 24 * 60 * 60 * 1000));
            } else {
                date = options.expires;
            }
            expires = '; expires=' + date.toUTCString(); // use expires attribute, max-age is not supported by IE
        }
        // CAUTION: Needed to parenthesize options.path and options.domain
        // in the following expressions, otherwise they evaluate to undefined
        // in the packed version for some reason...
        var path = options.path ? '; path=' + (options.path) : '';
        var domain = options.domain ? '; domain=' + (options.domain) : '';
        var secure = options.secure ? '; secure' : '';
        document.cookie = [name, '=', encodeURIComponent(value), expires, path, domain, secure].join('');
    } else { // only name given, get cookie
        var cookieValue = null;
        if (document.cookie && document.cookie != '') {
            var cookies = document.cookie.split(';');
            for (var i = 0; i < cookies.length; i++) {
                var cookie = jQuery.trim(cookies[i]);
                // Does this cookie string begin with the name we want?
                if (cookie.substring(0, name.length + 1) == (name + '=')) {
                    cookieValue = decodeURIComponent(cookie.substring(name.length + 1));
                    break;
                }
            }
        }
        return cookieValue;
    }
};

/*
 * jQuery Hotkeys Plugin
 * Copyright 2010, John Resig
 * Dual licensed under the MIT or GPL Version 2 licenses.
 *
 * Based upon the plugin by Tzury Bar Yochay:
 * http://github.com/tzuryby/hotkeys
 *
 * Original idea by:
 * Binny V A, http://www.openjs.com/scripts/events/keyboard_shortcuts/
*/

(function (jQuery) {

    jQuery.hotkeys = {
        version: "0.8",

        specialKeys: {
            8: "backspace", 9: "tab", 13: "return", 16: "shift", 17: "ctrl", 18: "alt", 19: "pause",
            20: "capslock", 27: "esc", 32: "space", 33: "pageup", 34: "pagedown", 35: "end", 36: "home",
            37: "left", 38: "up", 39: "right", 40: "down", 45: "insert", 46: "del",
            96: "0", 97: "1", 98: "2", 99: "3", 100: "4", 101: "5", 102: "6", 103: "7",
            104: "8", 105: "9", 106: "*", 107: "+", 109: "-", 110: ".", 111: "/",
            112: "f1", 113: "f2", 114: "f3", 115: "f4", 116: "f5", 117: "f6", 118: "f7", 119: "f8",
            120: "f9", 121: "f10", 122: "f11", 123: "f12", 144: "numlock", 145: "scroll", 191: "/", 224: "meta"
        },

        shiftNums: {
            "`": "~", "1": "!", "2": "@", "3": "#", "4": "$", "5": "%", "6": "^", "7": "&",
            "8": "*", "9": "(", "0": ")", "-": "_", "=": "+", ";": ": ", "'": "\"", ",": "<",
            ".": ">", "/": "?", "\\": "|"
        }
    };

    function keyHandler(handleObj) {
        // Only care when a possible input has been specified
        if (typeof handleObj.data !== "string") {
            return;
        }

        var origHandler = handleObj.handler,
			keys = handleObj.data.toLowerCase().split(" ");

        handleObj.handler = function (event) {
            // Don't fire in text-accepting inputs that we didn't directly bind to
            if (this !== event.target && (/textarea|select/i.test(event.target.nodeName) ||
				 event.target.type === "text")) {
                return;
            }

            // Keypress represents characters, not special keys
            var special = event.type !== "keypress" && jQuery.hotkeys.specialKeys[event.which],
				character = String.fromCharCode(event.which).toLowerCase(),
				key, modif = "", possible = {};

            // check combinations (alt|ctrl|shift+anything)
            if (event.altKey && special !== "alt") {
                modif += "alt+";
            }

            if (event.ctrlKey && special !== "ctrl") {
                modif += "ctrl+";
            }

            // TODO: Need to make sure this works consistently across platforms
            if (event.metaKey && !event.ctrlKey && special !== "meta") {
                modif += "meta+";
            }

            if (event.shiftKey && special !== "shift") {
                modif += "shift+";
            }

            if (special) {
                possible[modif + special] = true;

            } else {
                possible[modif + character] = true;
                possible[modif + jQuery.hotkeys.shiftNums[character]] = true;

                // "$" can be triggered as "Shift+4" or "Shift+$" or just "$"
                if (modif === "shift+") {
                    possible[jQuery.hotkeys.shiftNums[character]] = true;
                }
            }

            for (var i = 0, l = keys.length; i < l; i++) {
                if (possible[keys[i]]) {
                    return origHandler.apply(this, arguments);
                }
            }
        };
    }

    jQuery.each(["keydown", "keyup", "keypress"], function () {
        jQuery.event.special[this] = { add: keyHandler };
    });

})(jQuery);


//
//
// below is jquery.jeditable.mini.js
//
//


(function ($) {
    $.fn.editable = function (target, options) {
        if ('disable' == target) { $(this).data('disabled.editable', true); return; }
        if ('enable' == target) { $(this).data('disabled.editable', false); return; }
        if ('destroy' == target) { $(this).unbind($(this).data('event.editable')).removeData('disabled.editable').removeData('event.editable'); return; }
        var settings = $.extend({}, $.fn.editable.defaults, { target: target }, options); var plugin = $.editable.types[settings.type].plugin || function () { }; var submit = $.editable.types[settings.type].submit || function () { }; var buttons = $.editable.types[settings.type].buttons || $.editable.types['defaults'].buttons; var content = $.editable.types[settings.type].content || $.editable.types['defaults'].content; var element = $.editable.types[settings.type].element || $.editable.types['defaults'].element; var reset = $.editable.types[settings.type].reset || $.editable.types['defaults'].reset; var callback = settings.callback || function () { }; var onedit = settings.onedit || function () { }; var onsubmit = settings.onsubmit || function () { }; var onreset = settings.onreset || function () { }; var onerror = settings.onerror || reset; if (settings.tooltip) { $(this).attr('title', settings.tooltip); }
        settings.autowidth = 'auto' == settings.width; settings.autoheight = 'auto' == settings.height; return this.each(function () {
            var self = this; var savedwidth = $(self).width(); var savedheight = $(self).height(); $(this).data('event.editable', settings.event); if (!$.trim($(this).html())) { $(this).html(settings.placeholder); }
            $(this).bind(settings.event, function (e) {
                if (true === $(this).data('disabled.editable')) { return; }
                if (self.editing) { return; }
                if (false === onedit.apply(this, [settings, self])) { return; }
                e.preventDefault(); e.stopPropagation(); if (settings.tooltip) { $(self).removeAttr('title'); }
                if (0 == $(self).width()) { settings.width = savedwidth; settings.height = savedheight; } else {
                    if (settings.width != 'none') { settings.width = settings.autowidth ? $(self).width() : settings.width; }
                    if (settings.height != 'none') { settings.height = settings.autoheight ? $(self).height() : settings.height; }
                }
                if ($(this).html().toLowerCase().replace(/(;|")/g, '') == settings.placeholder.toLowerCase().replace(/(;|")/g, '')) { $(this).html(''); }
                self.editing = true; self.revert = $(self).html(); $(self).html(''); var form = $('<form />'); if (settings.cssclass) { if ('inherit' == settings.cssclass) { form.attr('class', $(self).attr('class')); } else { form.attr('class', settings.cssclass); } }
                if (settings.style) { if ('inherit' == settings.style) { form.attr('style', $(self).attr('style')); form.css('display', $(self).css('display')); } else { form.attr('style', settings.style); } }
                var input = element.apply(form, [settings, self]); var input_content; if (settings.loadurl) {
                    var t = setTimeout(function () { input.disabled = true; content.apply(form, [settings.loadtext, settings, self]); }, 100); var loaddata = {}; loaddata[settings.id] = self.id; if ($.isFunction(settings.loaddata)) { $.extend(loaddata, settings.loaddata.apply(self, [self.revert, settings])); } else { $.extend(loaddata, settings.loaddata); }
                    $.ajax({ type: settings.loadtype, url: settings.loadurl, data: loaddata, async: false, success: function (result) { window.clearTimeout(t); input_content = result; input.disabled = false; } });
                } else if (settings.data) { input_content = settings.data; if ($.isFunction(settings.data)) { input_content = settings.data.apply(self, [self.revert, settings]); } } else { input_content = self.revert; }
                content.apply(form, [input_content, settings, self]); input.attr('name', settings.name); buttons.apply(form, [settings, self]); $(self).append(form); plugin.apply(form, [settings, self]); $(':input:visible:enabled:first', form).focus(); if (settings.select) { input.select(); }
                input.keydown(function (e) { if (e.keyCode == 27) { e.preventDefault(); reset.apply(form, [settings, self]); } }); var t; if ('cancel' == settings.onblur) { input.blur(function (e) { t = setTimeout(function () { reset.apply(form, [settings, self]); }, 500); }); } else if ('submit' == settings.onblur) { input.blur(function (e) { t = setTimeout(function () { form.submit(); }, 200); }); } else if ($.isFunction(settings.onblur)) { input.blur(function (e) { settings.onblur.apply(self, [input.val(), settings]); }); } else { input.blur(function (e) { }); }
                form.submit(function (e) {
                    if (t) { clearTimeout(t); }
                    e.preventDefault(); if (false !== onsubmit.apply(form, [settings, self])) {
                        if (false !== submit.apply(form, [settings, self])) {
                            if ($.isFunction(settings.target)) { var str = settings.target.apply(self, [input.val(), settings]); $(self).html(str); self.editing = false; callback.apply(self, [self.innerHTML, settings]); if (!$.trim($(self).html())) { $(self).html(settings.placeholder); } } else {
                                var submitdata = {}; submitdata[settings.name] = input.val(); submitdata[settings.id] = self.id; if ($.isFunction(settings.submitdata)) { $.extend(submitdata, settings.submitdata.apply(self, [self.revert, settings])); } else { $.extend(submitdata, settings.submitdata); }
                                if ('PUT' == settings.method) { submitdata['_method'] = 'put'; }
                                $(self).html(settings.indicator); var ajaxoptions = {
                                    type: 'POST', data: submitdata, dataType: 'html', url: settings.target, success: function (result, status) {
                                        if (ajaxoptions.dataType == 'html') { $(self).html(result); }
                                        self.editing = false; callback.apply(self, [result, settings]); if (!$.trim($(self).html())) { $(self).html(settings.placeholder); }
                                    }, error: function (xhr, status, error) { onerror.apply(form, [settings, self, xhr]); }
                                }; $.extend(ajaxoptions, settings.ajaxoptions); $.ajax(ajaxoptions);
                            }
                        }
                    }
                    $(self).attr('title', settings.tooltip); return false;
                });
            }); this.reset = function (form) {
                if (this.editing) {
                    if (false !== onreset.apply(form, [settings, self])) {
                        $(self).html(self.revert); self.editing = false; if (!$.trim($(self).html())) { $(self).html(settings.placeholder); }
                        if (settings.tooltip) { $(self).attr('title', settings.tooltip); }
                    }
                }
            };
        });
    }; $.editable = {
        types: {
            defaults: {
                element: function (settings, original) { var input = $('<input type="hidden"></input>'); $(this).append(input); return (input); }, content: function (string, settings, original) { $(':input:first', this).val(string); }, reset: function (settings, original) { original.reset(this); }, buttons: function (settings, original) {
                    var form = this; if (settings.submit) {
                        if (settings.submit.match(/>$/)) { var submit = $(settings.submit).click(function () { if (submit.attr("type") != "submit") { form.submit(); } }); } else { var submit = $('<button type="submit" />'); submit.html(settings.submit); }
                        $(this).append(submit);
                    }
                    if (settings.cancel) {
                        if (settings.cancel.match(/>$/)) { var cancel = $(settings.cancel); } else { var cancel = $('<button type="cancel" />'); cancel.html(settings.cancel); }
                        $(this).append(cancel); $(cancel).click(function (event) {
                            if ($.isFunction($.editable.types[settings.type].reset)) { var reset = $.editable.types[settings.type].reset; } else { var reset = $.editable.types['defaults'].reset; }
                            reset.apply(form, [settings, original]); return false;
                        });
                    }
                }
            }, text: {
                element: function (settings, original) {
                    var input = $('<input />'); if (settings.width != 'none') { input.width(settings.width); }
                    if (settings.height != 'none') { input.height(settings.height); }
                    input.attr('autocomplete', 'off'); $(this).append(input); return (input);
                }
            }, textarea: {
                element: function (settings, original) {
                    var textarea = $('<textarea />'); if (settings.rows) { textarea.attr('rows', settings.rows); } else if (settings.height != "none") { textarea.height(settings.height); }
                    if (settings.cols) { textarea.attr('cols', settings.cols); } else if (settings.width != "none") { textarea.width(settings.width); }
                    $(this).append(textarea); return (textarea);
                }
            }, select: {
                element: function (settings, original) { var select = $('<select />'); $(this).append(select); return (select); }, content: function (data, settings, original) {
                    if (String == data.constructor) { eval('var json = ' + data); } else { var json = data; }
                    for (var key in json) {
                        if (!json.hasOwnProperty(key)) { continue; }
                        if ('selected' == key) { continue; }
                        var option = $('<option />').val(key).append(json[key]); $('select', this).append(option);
                    }
                    $('select', this).children().each(function () { if ($(this).val() == json['selected'] || $(this).text() == $.trim(original.revert)) { $(this).attr('selected', 'selected'); } });
                }
            }
        }, addInputType: function (name, input) { $.editable.types[name] = input; }
    }; $.fn.editable.defaults = { name: 'value', id: 'id', type: 'text', width: 'auto', height: 'auto', event: 'click.editable', onblur: 'cancel', loadtype: 'GET', loadtext: 'Loading...', placeholder: 'Click to edit', loaddata: {}, submitdata: {}, ajaxoptions: {} };
})(jQuery);