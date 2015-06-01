function loadShopIntoView() {
    return $.get("/Store/Store").success(function (data) {
        $("#words-section").html(data);
    });;
}

function addShopItemToList(shopItem) {
    var shopList = JSON.parse(getShopList());

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
    loadShopCartIntoView();
}

function deleteAllFromCart() {
    updateCart(Array());
    loadShopCartIntoView();
    loadShopIntoView();
}

function deleteFromCart(wordId) {
    var shopList = JSON.parse(getShopList());

    for (var i = 0; i < shopList.length; i++) {
        var item = shopList[i];

        if (item.WordId === wordId) {
            shopList.splice(i, 1);
        }
    }

    updateCart(shopList);
    loadShopCartIntoView();
}

function loadShopCartIntoView() {
    var shopList = getShopList();

    if (!shopList || shopList === "[]") {
        $("#shop-cart").html("");
        return;
    }

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

function buyWords() {
    var shopList = getShopList();

    $.ajax({
        cache: false,
        url: "Store/Buy",
        type: "POST",
        contentType: "application/json",
        data: shopList
    }).success(buyWordsSuccess).error(ajaxError);
}

function buyWordsSuccess(data) {
    data.errors.forEach(function (err) {
        $.notify({ message: err }, {
            type: "danger",
            placement:
                {
                    from: "top",
                    align: "center"
                }
        });
    });

    $.notify({ message: "Balance left: " + data.balance },
        {
            type: "success",
            placement: {
                from: "top",
                align: "center"
            }
        });

    updateCart(Array());

    loadShopIntoView();
    loadShopCartIntoView();
}

function ajaxError(error) {
    console.log(error);
    $.notify({ message: error },
        {
            type: "danger",
            placement: {
                from: "top",
                align: "center"
            }
        });
    loadShopIntoView();
    loadShopCartIntoView();
}

function updateCart(shopList) {
    shopList = JSON.stringify(shopList);
    localStorage.setItem("shopList", shopList);
}

function getShopList() {
    return localStorage.getItem("shopList") || "[]";
}
