﻿/**
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

//
// below is response-nav.min.js
//

/*! responsive-nav.js v1.0.20 by @viljamis, http://responsive-nav.com, MIT license */
var responsiveNav = function (h, g) {
    var v = !!h.getComputedStyle; h.getComputedStyle || (h.getComputedStyle = function (a) { this.el = a; this.getPropertyValue = function (b) { var c = /(\-([a-z]){1})/g; "float" === b && (b = "styleFloat"); c.test(b) && (b = b.replace(c, function (a, b, c) { return c.toUpperCase() })); return a.currentStyle[b] ? a.currentStyle[b] : null }; return this }); var d, f, e, n = g.createElement("style"), p, q, l = function (a, b, c, d) {
        if ("addEventListener" in a) try { a.addEventListener(b, c, d) } catch (e) {
            if ("object" === typeof c && c.handleEvent) a.addEventListener(b,
            function (a) { c.handleEvent.call(c, a) }, d); else throw e;
        } else "attachEvent" in a && ("object" === typeof c && c.handleEvent ? a.attachEvent("on" + b, function () { c.handleEvent.call(c) }) : a.attachEvent("on" + b, c))
    }, k = function (a, b, c, d) {
        if ("removeEventListener" in a) try { a.removeEventListener(b, c, d) } catch (e) { if ("object" === typeof c && c.handleEvent) a.removeEventListener(b, function (a) { c.handleEvent.call(c, a) }, d); else throw e; } else "detachEvent" in a && ("object" === typeof c && c.handleEvent ? a.detachEvent("on" + b, function () { c.handleEvent.call(c) }) :
        a.detachEvent("on" + b, c))
    }, w = function (a) { if (1 > a.children.length) throw Error("The Nav container has no containing elements"); for (var b = [], c = 0; c < a.children.length; c++) 1 === a.children[c].nodeType && b.push(a.children[c]); return b }, m = function (a, b) { for (var c in b) a.setAttribute(c, b[c]) }, r = function (a, b) { a.className += " " + b; a.className = a.className.replace(/(^\s*)|(\s*$)/g, "") }, s = function (a, b) { a.className = a.className.replace(RegExp("(\\s|^)" + b + "(\\s|$)"), " ").replace(/(^\s*)|(\s*$)/g, "") }, u = function (a, b) {
        var c;
        this.options = { animate: !0, transition: 350, label: "Menu", insert: "after", customToggle: "", openPos: "relative", jsClass: "js", init: function () { }, open: function () { }, close: function () { } }; for (c in b) this.options[c] = b[c]; r(g.documentElement, this.options.jsClass); this.wrapperEl = a.replace("#", ""); if (g.getElementById(this.wrapperEl)) this.wrapper = g.getElementById(this.wrapperEl); else throw Error("The nav element you are trying to select doesn't exist"); this.wrapper.inner = w(this.wrapper); f = this.options; d = this.wrapper;
        this._init(this)
    }; u.prototype = {
        destroy: function () { this._removeStyles(); s(d, "closed"); s(d, "opened"); d.removeAttribute("style"); d.removeAttribute("aria-hidden"); t = d = null; k(h, "resize", this, !1); k(g.body, "touchmove", this, !1); k(e, "touchstart", this, !1); k(e, "touchend", this, !1); k(e, "keyup", this, !1); k(e, "click", this, !1); k(e, "mouseup", this, !1); f.customToggle ? e.removeAttribute("aria-hidden") : e.parentNode.removeChild(e) }, toggle: function () {
            !0 === p && (q ? (s(d, "opened"), r(d, "closed"), m(d, { "aria-hidden": "true" }), f.animate ?
            (p = !1, setTimeout(function () { d.style.position = "absolute"; p = !0 }, f.transition + 10)) : d.style.position = "absolute", q = !1, f.close()) : (s(d, "closed"), r(d, "opened"), d.style.position = f.openPos, m(d, { "aria-hidden": "false" }), q = !0, f.open()))
        }, handleEvent: function (a) {
            a = a || h.event; switch (a.type) {
                case "touchstart": this._onTouchStart(a); break; case "touchmove": this._onTouchMove(a); break; case "touchend": case "mouseup": this._onTouchEnd(a); break; case "click": this._preventDefault(a); break; case "keyup": this._onKeyUp(a); break;
                case "resize": this._resize(a)
            }
        }, _init: function () { r(d, "closed"); p = !0; q = !1; this._createToggle(); this._transitions(); this._resize(); l(h, "resize", this, !1); l(g.body, "touchmove", this, !1); l(e, "touchstart", this, !1); l(e, "touchend", this, !1); l(e, "mouseup", this, !1); l(e, "keyup", this, !1); l(e, "click", this, !1); f.init() }, _createStyles: function () { n.parentNode || g.getElementsByTagName("head")[0].appendChild(n) }, _removeStyles: function () { n.parentNode && n.parentNode.removeChild(n) }, _createToggle: function () {
            if (f.customToggle) {
                var a =
                f.customToggle.replace("#", ""); if (g.getElementById(a)) e = g.getElementById(a); else throw Error("The custom nav toggle you are trying to select doesn't exist");
            } else a = g.createElement("a"), a.innerHTML = f.label, m(a, { href: "#", id: "nav-toggle" }), "after" === f.insert ? d.parentNode.insertBefore(a, d.nextSibling) : d.parentNode.insertBefore(a, d), e = g.getElementById("nav-toggle")
        }, _preventDefault: function (a) { a.preventDefault ? (a.preventDefault(), a.stopPropagation()) : a.returnValue = !1 }, _onTouchStart: function (a) {
            a.stopPropagation();
            this.startX = a.touches[0].clientX; this.startY = a.touches[0].clientY; this.touchHasMoved = !1; k(e, "mouseup", this, !1)
        }, _onTouchMove: function (a) { if (10 < Math.abs(a.touches[0].clientX - this.startX) || 10 < Math.abs(a.touches[0].clientY - this.startY)) this.touchHasMoved = !0 }, _onTouchEnd: function (a) {
            this._preventDefault(a); if (!this.touchHasMoved) if ("touchend" === a.type) {
                this.toggle(a); var b = this; d.addEventListener("click", b._preventDefault, !0); setTimeout(function () { d.removeEventListener("click", b._preventDefault, !0) },
                f.transition + 100)
            } else { var c = a || h.event; 3 !== c.which && 2 !== c.button && this.toggle(a) }
        }, _onKeyUp: function (a) { 13 === (a || h.event).keyCode && this.toggle(a) }, _transitions: function () { if (f.animate) { var a = d.style, b = "max-height " + f.transition + "ms"; a.WebkitTransition = b; a.MozTransition = b; a.OTransition = b; a.transition = b } }, _calcHeight: function () { for (var a = 0, b = 0; b < d.inner.length; b++) a += d.inner[b].offsetHeight; a = "#" + this.wrapperEl + ".opened{max-height:" + a + "px}"; v && (n.innerHTML = a) }, _resize: function () {
            "none" !== h.getComputedStyle(e,
            null).getPropertyValue("display") ? (m(e, { "aria-hidden": "false" }), d.className.match(/(^|\s)closed(\s|$)/) && (m(d, { "aria-hidden": "true" }), d.style.position = "absolute"), this._createStyles(), this._calcHeight()) : (m(e, { "aria-hidden": "true" }), m(d, { "aria-hidden": "false" }), d.style.position = f.openPos, this._removeStyles())
        }
    }; var t; return function (a, b) { t || (t = new u(a, b)); return t }
}(window, document);
