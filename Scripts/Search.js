let adminAdvancedSearch = {

  resultsTimeout: null,

  currentQuery: null,

  init: function () {
    let input = document.querySelector('#search-box input');
    if (input instanceof HTMLInputElement)
      input.addEventListener('keyup', function (e) {
        if (e instanceof Event)
          adminAdvancedSearch.getResults(e.target.value, e.key == 'Enter');
      });
  },

  getResults: function (searchQuery, refresh = false) {

    if (searchQuery == adminAdvancedSearch.currentQuery &&
      !refresh)
      return;

    clearTimeout(adminAdvancedSearch.resultsTimeout);

    adminAdvancedSearch.resultsTimeout = setTimeout(function () {
      let fd = new FormData();
      fd.set('searchQuery', searchQuery);

      let tokenElems = document.getElementsByName('__RequestVerificationToken');
      if (tokenElems.length > 0 && tokenElems[0] instanceof HTMLInputElement)
        fd.set('__RequestVerificationToken', tokenElems[0].value)

      fetch('/Admin/AdvancedAdminSearch/GetResults', adminAdvancedSearch.postFormObject(fd))
        .then(res => adminAdvancedSearch.result(res))
        .then(res => {

          if (res.status && res.results !== undefined) {
            var results = adminAdvancedSearch.parseResults(res.results);
            adminAdvancedSearch.injectResults(results);
            adminAdvancedSearch.currentQuery = res.SearchQuery;
          } else {
            console.log(res);
            let emptyMessage = document.querySelector('.tt-dataset.tt-dataset-pages .empty-message');
            if (emptyMessage instanceof HTMLElement)
              emptyMessage.style.display = 'block';
          }

        })
    }, 150);

  },

  injectResults: function (results) {

    for (let elem of document.querySelectorAll('.tt-dataset.tt-dataset-pages .adv-result')) {
      if (elem instanceof HTMLElement)
        elem.outerHTML = '';
    }

    let list = document.querySelector('.tt-dataset.tt-dataset-pages');
    for (let result of results) {
      list.appendChild(result);
    }

    let emptyMessage = document.querySelector('.tt-dataset.tt-dataset-pages .empty-message');
    if (emptyMessage instanceof HTMLElement)
      emptyMessage.style.display = 'none';
  },

  parseResults: function (data) {

    let results = [];

    let orders = data.OrderResults;
    if (orders instanceof Array && orders.length > 0) {
      results.push(adminAdvancedSearch.titleNode('Orders'));
      for (let item of orders) {
        results.push(adminAdvancedSearch.resultToNode(item));
      }
    }

    let customers = data.CustomerResults;
    if (customers instanceof Array && customers.length > 0) {
      results.push(adminAdvancedSearch.titleNode('Customers'));
      for (let item of customers) {
        results.push(adminAdvancedSearch.resultToNode(item));
      }
    }

    let products = data.ProductResults;
    if (products instanceof Array && products.length > 0) {
      results.push(adminAdvancedSearch.titleNode('Products'));
      for (let item of products) {
        results.push(adminAdvancedSearch.resultToNode(item));
      }
    }

    return results;
  },

  titleNode: function (title) {
    let div = document.createElement('div');
    div.id = 'user-selection';
    div.classList.add('tt-suggestion', 'tt-selectable', 'adv-result');
    div.innerText = title;
    return div;
  },

  resultToNode: function (result) {
    let div = document.createElement('div');
    div.id = 'user-selection';
    div.classList.add('tt-suggestion', 'tt-selectable', 'adv-result');
    div.setAttribute('onclick', `window.location="${result.ResultUrl}"`);
    div.innerHTML = `<h5>${result.ResultTitle}</h5>`;
    return div;
  },

  loading: function () {

  },

  postObject: function (data) {
    return {
      method: 'POST',
      headers: adminAdvancedSearch.jsonHeaders,
      body: data != null && data.constructor !== ''.constructor ? JSON.stringify(data) : data
    };
  },

  postFormObject: function (data, e = null) {

    let formData = data instanceof HTMLFormElement ? new FormData(data) : (data instanceof FormData ? data : null);

    if (e != null)
      formData.set(e.submitter.name, e.submitter.value);

    return {
      method: 'POST',
      body: formData
    };
  },

  jsonHeaders: {
    'Accept': 'application/json',
    'Content-Type': 'application/json'
  },

  result: function (fetchResult) {

    if (fetchResult.ok) {

      let contentType = fetchResult.headers.get('content-type');

      if (contentType == null)
        contentType = fetchResult.headers.get('Content-Type');

      if (contentType == null) {
        console.error('Content type was null...', fetchResult);
        return null;
      }

      if (contentType.includes('text/html')) {
        return adminAdvancedSearch.textResult(fetchResult);
      } else if (contentType.includes('application/json')) {
        return adminAdvancedSearch.jsonResult(fetchResult);
      } else {
        console.log('content type was unfound.', contentType);
        return null;
      }

    } else {
      if (fetchResult.status == 404) {
        console.error('Request failed because page wasnt found');
        return false;
      } else {
        console.error('Request error ' + fetchResult.status);
        return fetchResult.text().then(x => {
          if (x.includes('anti-forgery token') || x.includes('anti-forgery cookie') || x.includes('__RequestVerificationToken')) {
            alert('** Form token error. **\nYou must refresh the page and try submitting the form again.');
          }
          return false;
        });
      }
    }
  },

  jsonResult: function (fetchResult) {
    if (fetchResult.ok) {
      return fetchResult.json().then(x => {
        x.isJson = true;
        return x;
      });
    }
  },

  textResult: function (fetchResult) {
    if (fetchResult.ok) {
      return fetchResult.text();
    } else {
      if (fetchResult.status == 404) {
        console.error('Request failed because page wasnt found');
        return false;
      } else {
        console.error('Request error ' + fetchResult.status);
        return fetchResult.text().then(x => {
          if (x.includes('anti-forgery token') || x.includes('anti-forgery cookie') || x.includes('__RequestVerificationToken')) {
            alert('** Form token error. **\nYou must refresh the page and try submitting the form again.');
          }
          return false;
        });
      }
    }
  },
};

adminAdvancedSearch.init();