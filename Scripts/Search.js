let adminAdvancedSearch = {

  resultsTimeout: null,

  currentQuery: null,

  activePromises: [],

  loaderElem: null,

  init: () => {
    let input = document.querySelector('#search-box input');
    if (input instanceof HTMLInputElement) {
      ['input', 'change', 'paste', 'keyup', 'mouseup'].forEach(eventType => {
        input.addEventListener(eventType, (e) => {
          if (e instanceof Event)
            adminAdvancedSearch.getResults(e.target.value, e.key == 'Enter');
        }, false);
      });
    }

  },

  initLoader: () => {
    adminAdvancedSearch.loaderElem = document.getElementById('list-loader');

    if (!(adminAdvancedSearch.loaderElem instanceof HTMLElement)) {
      let div = document.createElement('div');
      div.id = 'list-loader';
      div.appendChild(document.createElement('div'))
      let list = document.querySelector('.tt-dataset.tt-dataset-pages');
      list.prepend(div);

      adminAdvancedSearch.loaderElem = document.getElementById('list-loader');
    }
  },

  getResults: (searchQuery, refresh = false) => {

    let lastSearch = adminAdvancedSearch.activePromises[adminAdvancedSearch.activePromises.length - 1];

    adminAdvancedSearch.initLoader();

    if ((lastSearch != undefined && searchQuery == lastSearch.Query) || searchQuery == '') {
      adminAdvancedSearch.loaderElem.classList.remove('loading');
      return;
    }

    if (adminAdvancedSearch.resultsTimeout != null) {
      clearTimeout(adminAdvancedSearch.resultsTimeout);
      let lastRequest = adminAdvancedSearch.activePromises.find((item) => item.Id == adminAdvancedSearch.resultsTimeout);
      if (lastRequest != undefined) {
        lastRequest.Controller.abort();
        adminAdvancedSearch.activePromises.splice(adminAdvancedSearch.activePromises.indexOf(lastRequest), 1)
      }
    }

    adminAdvancedSearch.resultsTimeout = setTimeout(() => {
      let fd = new FormData();
      fd.set('searchQuery', searchQuery);

      let activeTimeout = adminAdvancedSearch.resultsTimeout;

      let tokenElems = document.getElementsByName('__RequestVerificationToken');
      if (tokenElems.length > 0 && tokenElems[0] instanceof HTMLInputElement)
        fd.set('__RequestVerificationToken', tokenElems[0].value)

      let abortController = new AbortController();

      adminAdvancedSearch.activePromises.push({ Id: activeTimeout, Controller: abortController, Query: searchQuery });

      adminAdvancedSearch.loaderElem.classList.add('loading');

      fetch('/Admin/AdvancedAdminSearch/GetResults', adminAdvancedSearch.postFormObject(fd, abortController.signal))
        .then(res => adminAdvancedSearch.result(res))
        .then(res => {

          if (res.status && res.results !== undefined) {
            adminAdvancedSearch.loaderElem.classList.remove('loading');
            let results = adminAdvancedSearch.parseResults(res.results);
            adminAdvancedSearch.injectResults(results);
            adminAdvancedSearch.currentQuery = res.SearchQuery;
          } else {
            let emptyMessage = document.querySelector('.tt-dataset.tt-dataset-pages .empty-message');
            if (emptyMessage instanceof HTMLElement)
              emptyMessage.style.display = 'block';
          }

        })
        .catch(err => {
          if (err instanceof DOMException) {
            return;
          }
        });

    }, 300);

  },

  injectResults: (results) => {

    for (let elem of document.querySelectorAll('.tt-dataset.tt-dataset-pages .adv-result')) {
      if (elem instanceof HTMLElement)
        elem.outerHTML = '';
    }

    let list = document.querySelector('.tt-dataset.tt-dataset-pages');
    if (list instanceof HTMLElement)
      list.append(...results);

    let emptyMessage = document.querySelector('.tt-dataset.tt-dataset-pages .empty-message');
    if (emptyMessage instanceof HTMLElement)
      emptyMessage.style.display = 'none';
  },

  parseResults: (data) => {

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

  titleNode: (title) => {
    let div = document.createElement('div');
    div.id = 'user-selection';
    div.classList.add('tt-suggestion', 'tt-selectable', 'adv-result');
    div.innerText = title;
    div.style.fontWeight = '600';
    div.style.padding = '5px 0';
    div.style.textAlign = 'center';
    div.style.color = '#007bff';
    return div;
  },

  resultToNode: (result) => {
    let div = document.createElement('div');
    div.id = 'user-selection';
    div.classList.add('tt-suggestion', 'tt-selectable', 'adv-result');
    div.setAttribute('onclick', `window.location="${result.ResultUrl}"`);
    div.innerHTML = `<h5>${result.ResultTitle}</h5>`;
    return div;
  },

  postObject: (data) => ({
    method: 'POST',
    headers: adminAdvancedSearch.jsonHeaders,
    body: data != null && data.constructor !== ''.constructor ? JSON.stringify(data) : data
  }),

  postFormObject: (data, signal = null, e = null) => {

    let formData = data instanceof HTMLFormElement ? new FormData(data) : (data instanceof FormData ? data : null);

    if (e != null)
      formData.set(e.submitter.name, e.submitter.value);

    return {
      method: 'POST',
      body: formData,
      signal: signal
    };
  },

  jsonHeaders: {
    'Accept': 'application/json',
    'Content-Type': 'application/json'
  },

  result: (fetchResult) => {

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

  jsonResult: (fetchResult) => {
    if (fetchResult.ok) {
      return fetchResult.json().then(x => {
        x.isJson = true;
        return x;
      });
    }
  },

  textResult: (fetchResult) => {
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