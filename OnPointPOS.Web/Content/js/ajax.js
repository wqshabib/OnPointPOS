/*! main.js - v0.1.1
* http://admindesigns.com/
* Copyright (c) 2013 Admin Designs;*/

/* Core theme functions required for
 * most of the themes vital functionality */
var Ajax = function () {	
	
	// Declare needed global variables	
	var pageURL,
	    ajaxLink,
	    ajaxTitle,	
	    ajaxURL,
	    ajaxHash;
			
    // Ajax Page Loading
    var runPageload = function () {
								
		// This controls the settings and injection of the ajax loading spinner			
		function loadSpinner() {	
			// Only inject the spinner if the plugin has been loaded	
			if (window.Spinner) {	
				// Ensures only one spinner is active at a time
				if (!$('#spinner').length) {$('body').append('<div id="spinner"></div>');}
								
				// Spinner settings
				var opts = {
				  lines: 11, // The number of lines to draw
				  length: 9, // The length of each line
				  width: 5, // The line thickness
				  radius: 12, // The radius of the inner circle
				  corners: 0.4, // Corner roundness (0..1)
				  color: '#a87fc6', // #rgb or #rrggbb or array of colors
				  className: 'spinner', // The CSS class to assign to the spinner
				  top: '45%', // Top position relative to parent
				  left: '55%' // Left position relative to parent
				};
				var target = document.getElementById('spinner');
				var spinner = new Spinner(opts).spin(target);
			}
		};
		
		// Ajax loading often involves loading the same plugin multiple times(different pages, etc).
		// However some plugins can't be reinitilized on existing DOM elements or they will error. 
		// This function list those plugins and detaches their handlers on load so that their 
		// elements are free to be reattached to at a later time
		function pluginUnload() {
	
			// List of plugins that require detachment
			var pluginList = ['.datepicker', '.rangepicker', '.colorpicker', '.rgbapicker', '.timepicker', '#graph'];
			
			// Cycle through each plugin and detach its handler
			$.each(pluginList, function(i, e) {
				if ($(e).length) {
				  $(e).detach();
				};
			});
		};
		
		// We Ajax load the requested content and then prepare the main container for its arrival. 
		// Declared as a global function as Google API utilizes this script from the window scope.
		var loadcontent = function() {		
		  $.ajax({
			    // Adds a new body class, preps the content container, and adds a css animation
				beforeSend: function() {
				  $('#content_wrapper').addClass('animated animated-shortest fadeOut');
				  $('#defaultModal, #formModal, #alertModal').remove();
				  loadSpinner();  
				  $('body').removeClass(pageURL.toLowerCase() + " " + ajaxHash.substring(1) + '-page').addClass(ajaxTitle + '-page'); 
				},
				url: ajaxURL,
				cache:true,
				// On Success detach various handlers from DOM to prevent plugin errors
				success: function(data) {
				   pluginUnload();
				   $('#content_wrapper').html(data);
				},
				// On Complete reInit checkbox plugin and remove css animations
				complete: function() {
				   $('#content_wrapper').removeClass('fadeOut').addClass('animated animated-shortest fadeInUp');	
				   $('#spinner').empty();
						
				  setTimeout(function() {
				      $('#content').removeClass('animated animated-shortest fadeIn');
				  },1000);
		
				//   $('#content_wrapper').one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function(){
				//	  $('#content_wrapper').removeClass('animated animated-shortest fadeInUp fadeOut');
				 //  });
				  
				}	  
		  });
		};
		
		jQuery.ajaxSetup({cache:true});
			
		// Attach handler to intercept menu item events - Left Sidebar, Left User Menu, Right User Menu 
		$('.sidebar-menu li a, .user-menu a, .sidebar_right_menu a').on('click', function(e) {
								
			// Check for scenarios where we might want to prevent an ajax load
			if ($(e.currentTarget).hasClass('accordion-toggle')) {
				 return;
			} // We prevent load when user clicks a dropdown link
			if ($('#ajax-option').length && !$('#ajax-option').prop('checked')) {return;} // We prevent load if the ajax option checkbox is unchecked
			if ($(e.currentTarget).hasClass('ajax-disable')) {return;} // Manually disable links via html class. Used on large docs
		
			// Disable item click to prevent page refresh
			e.preventDefault();
			
			
			// Declare several needed global variables
			pageURL = window.location.pathname.slice(0, -5).split('/').pop();
			ajaxLink = $(e.currentTarget).attr('href');
			ajaxTitle = ajaxLink.slice(0, -5);	
			ajaxURL = 'ajax/' + ajaxLink;
			ajaxHash = window.location.hash;
		
			// On ajax load we create a url hash which helps track content
			// If the hash is unchanged we prevent load(prevents multiple spam clicks)
			if ('#' + ajaxTitle == ajaxHash) {return;}
		
			// Add Hash to track ajax page
			window.location.hash = ajaxTitle;
			
			// Use URL Hash to find and remove the old active menu item
			$('a[href="'+ajaxHash.substring(1)+'.html"]').parent('li').removeClass('active');	
		
			// Apply the ".active" class to the clicked menu item
			$(e.currentTarget).parent('li').addClass('active')
 
		    loadcontent(); 
		});			
    }
	return {
        init: function () {
            runPageload();
        }
	} 
}();


