/*! main.js - v0.1.1
* http://admindesigns.com/
* Copyright (c) 2013 Admin Designs;*/

/* Core theme functions required for
 * most of the themes vital functionality */
var Widgets = function () {

	// Form related Functions
    var runPanels = function () {
		
	  // On Load Widget Check
	  $('[data-panel-fullscreen="true"]').each(function (i,e) {
		  alert("yup");
			createMenu(e);
	  });
	  

	  // CREATE WIDGET MENUS
	  ///////////////////////////
	 	
	  // Create Menu
	  function createMenu(e) {
		  var htmlObject = $(e);
		  
		  // List of Appendable Menus and Buttons
		  var buttonFullscreen = '<span class="pull-right mr25 widget-menu widget-menu-fullscreen"><i class="fa fa-arrows-h"></i></span>';	

		  // Call function to create Menu	
		  $(htmlObject).find('.panel-heading').append(buttonFullscreen);
		  
		  // Respond with Appropriate action on Panel Menu Click 	
		  $('.panel-heading').on('click', '.widget-menu', function() {
				var This = $(this);
				if (This.hasClass('widget-menu-fullscreen')) {
					actionFullscreen(This);
				};
		  });
		 
	  };
		
	  // Assign Menu Job	
	  function assignAction(e) {
		  var htmlObject = $(e);
		  
		  // List of Appendable Menus and Buttons
		  var buttonFullscreen = '<span class="pull-right mr25 widget-menu widget-menu-fullscreen"><i class="fa fa-arrows-h"></i></span>';	

		  // Call function to create Menu	
		  $(htmlObject).find('.panel-heading').append(buttonFullscreen);
		  
		  // Respond with Appropriate action on Panel Menu Click 	
		  $('.panel-heading').on('click', '.widget-menu', function() {
				var This = $(this);
				if (This.hasClass('widget-menu-fullscreen')) {
					actionFullscreen(This);
				};
		  });
		 
	  };			
	
	  // WIDGET MENU ACTIONS
	  //////////////////////////	
		
	 				
	  // Fullscreen Action
	  function actionFullscreen(o) {
		  
		 var panel = $(o).parent('.panel-heading').parent('.panel');
		  
		 if (panel.hasClass('widget-fullscreen')) {
			 panel.removeClass('widget-fullscreen');
			 themeModal('destroy');
		 }
		 else {
			 panel.addClass('widget-fullscreen');
			 themeModal('create');
		 }
	  };
	  
	  
	  // Action Helper Functions
	  ///////////////////////////
	  
	  // Calls Modal. Accepts Params: "destroy" and "create"
	  function themeModal(action) {
		  
		 // Create Modal
		 if (action == "create") {
			 $('body').append('<div class="widget-backdrop"></div>');
		 }
		 
		 // Destroy Modal
		 if (action == "destroy") {
			 $('.widget-backdrop').remove();
		 }
		 
		 // Destroy Modal On Click(outside of content) 
		 $('body').on('click', '.widget-backdrop', function() {
			 $('.widget-backdrop').remove();
			 
			 // Also check for any widgets in fullscreen mode and close them
			 $('.panel.widget-fullscreen').removeClass('widget-fullscreen');
		 });
		 
	  };
		
	  	
		
		
		
		
	  $('.fusion-panel').each(function () {
		  var This = $(this)
		  
		  // Widget Data Variables
		  var widgetHeight = This.data('panel-height');
		  var widgetScroll = This.data('panel-scroll');

		  // Widget Heights
		  var widgetHeader = This.children('.panel-heading').outerHeight();
		  var widgetMenu = This.children('.panel-menu').outerHeight();
		  var widgetBody = This.children('.panel-body').outerHeight();
		  var widgetFooter = This.children('.panel-footer').outerHeight();
		  
		//  alert(widgetHeader);	  
		//  alert(widgetBody);
		//  alert(widgetFooter);
		  
		  // Used to Calculate size of panel/widget body
		  var widgetTotal = (widgetHeader + widgetMenu + widgetFooter);
		  
		  // Define Widget Height
 		  if (widgetHeight) { widgetAdjust_Height(); }
		  
		  // Define Widget Overflow
		  if (widgetScroll) { widgetAdjust_Scroll(); }
		  
		  // Adjust Widget Height
		  function widgetAdjust_Height() {
			  This.children('.panel-body').height(widgetHeight - widgetTotal );
		  };
		  
		  // Define Widget Overflow
		  function widgetAdjust_Scroll() {
			  This.children('.panel-body').css('overflow-y', 'auto'); 
			  if (This.children('.panel-tray')) {
				   This.children('.panel-body').css('overflow-x', 'hidden'); 
				   This.find('.panel-tray').height(widgetBody);
			  }
		  };
		  
		  
	  });
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		
		

  	   // This function controls the opening and closing of
	   // panel trays, see docs for more information
       $("#content_wrapper").on('click', '.panel-tray-toggle', function () {
          $(this).parents(".panel-tray").toggleClass("tray-open");
       });
		
		
		
		
    }

	return {
        init: function () {
            runPanels();
        }
	} 
}();


