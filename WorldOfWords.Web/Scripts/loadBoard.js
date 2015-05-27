var app = app || {};

var loader = function(board){
    var json = '{"content":"куче о  х т носк    а    ","words":["куче","котка","ехо","нос"]}',
    json2 = '{"content":" к    о    т    к    а   ","words":["котка"]}';
    board.loadBoard(json2);
}(app.board);