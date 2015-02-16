(function () {
    var VotingApp = angular.module('VotingApp');

    VotingApp.controller('BasicVoteController', function ($scope) {
        $scope.options = [
            { name: "Option 1", description: "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nunc congue ut lacus a vehicula. Ut accumsan elementum hendrerit. Pellentesque gravida tortor libero, vel tristique nunc hendrerit interdum. Cras in ligula sit amet nibh accumsan sollicitudin eu ut dui. Phasellus posuere turpis ac mi sagittis, in pharetra massa sodales. Integer sagittis, mi eu varius vulputate, libero turpis semper arcu, vitae rhoncus turpis justo sed ex. Praesent hendrerit velit non maximus vulputate. Vestibulum imperdiet ipsum sit amet molestie lacinia. Aliquam auctor malesuada enim, sed porta metus vulputate tristique." },
            { name: "Option 2", description: "Integer volutpat quam massa, id tempus augue hendrerit varius. Nulla facilisi. Etiam ullamcorper congue lorem vel pellentesque. Integer eget rhoncus felis. Morbi faucibus pulvinar congue. Sed semper pellentesque nisi. Fusce sit amet augue convallis, pharetra sapien sit amet, tristique diam. In imperdiet tempus mauris, at blandit sem volutpat malesuada. Vivamus bibendum velit a mauris sagittis, sed mollis justo laoreet. Sed bibendum pretium lacus sit amet volutpat. Phasellus eu tincidunt leo, sit amet venenatis velit. In et sem ligula." }
        ];
    });

})();
