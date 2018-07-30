// Author: Stephen Korecky
// Website: http://stephenkorecky.com
// Plugin Website: http://github.com/skorecky/Add-Clear
jQ = jQuery;
; (function ($, window, document, undefined) {
	// Create the defaults once
	var pluginName = "addClear",
		defaults = {
			closeSymbol: "&#10006;",
			color: "#CCC",
			top: 1,
			right: 4,
			returnFocus: true,
			showOnLoad: false,
			onClear: null,
			hideOnBlur: false
		};

	// The actual plugin constructor
	function Plugin(element, options) {
		this.element = element;

		this.options = $.extend({}, defaults, options);

		this._defaults = defaults;
		this._name = pluginName;

		this.init();
	}

	Plugin.prototype = {

		init: function () {
			var $this = jQ(this.element),
					me = this,
					options = this.options;

			$this.wrap("<span style='position:relative;' class='add-clear-span'></span>");
			$this.after(jQ("<a href='#clear' style='display: none;'>" + options.closeSymbol + "</a>"));
			$this.next().css({
				'font-family': 'courier new',
				color: options.color,
				'text-decoration': 'none',
				display: 'none',
				'line-height': 1,
				overflow: 'hidden',
				position: 'absolute',
				right: options.right,
				top: options.top + 4        // ”÷’²®
			}, this);

			if ($this.val().length >= 1 && options.showOnLoad === true) {
				$this.siblings("a[href='#clear']").show();
			}

			$this.focus(function () {
				if (jQ(this).val().length >= 1) {
					jQ(this).siblings("a[href='#clear']").show();
				}
			});

			$this.blur(function () {
				var self = this;

				if (options.hideOnBlur) {
					setTimeout(function () {
						jQ(self).siblings("a[href='#clear']").hide();
					}, 50);
				}
			});

			$this.keyup(function () {
				if (jQ(this).val().length >= 1) {
					jQ(this).siblings("a[href='#clear']").show();
				} else {
					jQ(this).siblings("a[href='#clear']").hide();
				}
			});

			jQ("a[href='#clear']").click(function (e) {
				jQ(this).siblings(me.element).val("");
				jQ(this).hide();
				if (options.returnFocus === true) {
					jQ(this).siblings(me.element).focus();
				}
				if (options.onClear) {
					options.onClear(jQ(this).siblings("input"));
				}
				e.preventDefault();
			});
		}

	};

	$.fn[pluginName] = function (options) {
		return this.each(function () {
			if (!$.data(this, "plugin_" + pluginName)) {
				$.data(this, "plugin_" + pluginName,
					new Plugin(this, options));
			}
		});
	};

})(jQuery, window, document);
