function loadShop() {
    return $.get("/Store/Store");
}

function addShopItemToList(shopItem, shopList) {
    var indexOfItemInArray = -1;
    for (var i = 0; i < shopList.length; i++) {
        var item = shopList[i];
        if (shopItem.WordId === item.WordId) {
            indexOfItemInArray = i;
            break;
        }
    }

    if (indexOfItemInArray !== -1) {
        shopList.splice(indexOfItemInArray, 1);
    }

    shopList.push(shopItem);
}

function deleteShopItemFromList(wordId, shopList) {
    for (var i = 0; i < shopList.length; i++) {
        var item = shopList[i];

        if (item.WordId === wordId) {
            shopList.splice(i, 1);
        }
    }
}

function loadShopCartView(shopList) {
    shopList = JSON.stringify(shopList);

    return $.ajax({
        cache: false,
        url: "Store/Cart",
        type: "POST",
        contentType: "application/json",
        data: shopList
    }).error(ajaxError);
}

function buyWords(shopList) {
    shopList = JSON.stringify(shopList);

    $.ajax({
        cache: false,
        url: "Store/Buy",
        type: "POST",
        contentType: "application/json",
        data: shopList
    }).success(function (data) {
        var output = "";
        
        data.errors.forEach(function(err) {
            output += err + "\n";
        });

        output += "Balance left: " + data.balance;

        alert(output);
        location.reload();
    }).error(ajaxError);
}

function ajaxError(error) {
    alert("Error: " + error.statusText);
    location.reload();
}