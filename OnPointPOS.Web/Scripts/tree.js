(function ($) {
    function Tree() {
        var $this = this;

        function treeNodeClick() {
            $(document).on('click', '.tree li a input[type="checkbox"]', function () {
                $(this).closest('li').find('ul input[type="checkbox"]').prop('checked', $(this).is(':checked'));
            }).on('click', '.node-item', function () {
                var parentNode = $(this).parents('.tree ul');
                if ($(this).is(':checked')) {
                    parentNode.find('li a .parent').prop('checked', true);
                } else {
                    var elements = parentNode.find('ul input[type="checkbox"]:checked');
                    if (elements.length == 0) {
                        parentNode.find('li a .parent').prop('checked', false);
                    }
                }
            });
        };

        $this.init = function () {
            treeNodeClick();
        }
    }
    $(function () {
        var self = new Tree();
        self.init();
    })
}(jQuery))