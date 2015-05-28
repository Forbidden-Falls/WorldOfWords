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

    updateCart(shopList);
}

function deleteAllFromCart(shopList) {
    shopList = [];

    updateCart(shopList).success(loadShopCartIntoView);
    location.reload();
}

function deleteFromCart(wordId, shopList) {
    deleteShopItemFromList(wordId, shopList).success(loadShopCartIntoView);
}

function deleteShopItemFromList(wordId, shopList) {
    for (var i = 0; i < shopList.length; i++) {
        var item = shopList[i];

        if (item.WordId === wordId) {
            shopList.splice(i, 1);
        }
    }

    return updateCart(shopList);
}

function loadShopCartIntoView(shopList) {
    if (shopList.shopList === null) {
        $("#shop-cart").html("");
        return;
    }

    shopList = JSON.stringify(shopList); 

    $.ajax({
        cache: false,
        url: "Store/Cart",
        type: "POST",
        contentType: "application/json",
        data: shopList
    }).success(function (data) {
        $("#shop-cart").html(data);
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
    }).success(buyWordsSuccess).error(ajaxError);
}

function updateCart(shopList) {
    shopList = JSON.stringify(shopList);

    return $.ajax({
        cache: false,
        url: "Store/UpdateCart",
        type: "POST",
        contentType: "application/json",
        data: shopList
    });
}

function buyWordsSuccess(data) {
    var output = "";

    data.errors.forEach(function (err) {
        output += err + "\n";
    });

    output += "Balance left: " + data.balance;

    alert(output);
    location.reload();
}

function ajaxError(error) {
    alert("Error: " + error.statusText);
    location.reload();
}