import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { runInNewContext } from 'node:vm';
import { parseInvite, inviteCode } from '../wwwroot/invite.js';

test('download page offers desktop software and keeps shared board keys out of requests',async()=>{
  const html=await readFile(new URL('../wwwroot/index.html',import.meta.url),'utf8');
  const js=await readFile(new URL('../wwwroot/download.js',import.meta.url),'utf8');
  assert.ok(!/<canvas\b|data-tool=|app\.js/.test(html));
  for(const platform of ['windows','linux','arch','void','macos'])assert.ok(html.includes(`id="${platform}"`));
  assert.ok(!js.includes('/api/boards'));
  assert.ok(js.includes("credentials:'omit'")&&js.includes("referrerPolicy:'no-referrer'"));
  assert.ok(html.includes('releases/latest'));
});

async function downloadPage(language,assets,{hash='',storedLanguage=null,storageDenied=false}={}) {
  const source=await readFile(new URL('../wwwroot/download.js',import.meta.url),'utf8');
  function element(text='') {
    return {href:'https://github.com/Xekep/NovaPen/releases/latest',textContent:text,
      dataset:{},attributes:{},classList:{toggle(){}},parentElement:{prepend(){}},listeners:{},
      setAttribute(name,value){this.attributes[name]=value;},
      addEventListener(name,handler){this.listeners[name]=handler;},
      querySelector(){return {textContent:text};},
      replaceChildren(...children){this.textContent=children.map(c=>c.textContent||'').join('');}};
  }
  const links=Object.fromEntries(['windows','linux','arch','void','macos','invitation','copy-invite','language'].map(id=>[id,element(id)]));
  const heading=element();heading.dataset.text='download';
  const title=element();title.dataset.text='title';
  const meta=element(),downloads=element();
  const document={documentElement:{},getElementById:id=>links[id],
    querySelector:selector=>selector.startsWith('meta')?meta:downloads,
    querySelectorAll:selector=>selector==='[data-text]'?[heading,title]:['windows','linux','arch','void','macos'].map(id=>links[id]),
    createTextNode:text=>({textContent:text}),createElement:()=>({textContent:''})};
  const requests=[],storage=[],copied=[],events={};
  runInNewContext(source.replace(/^import .*;\r?\n/,''),{
    parseInvite,inviteCode,navigator:{language,platform:'Linux',clipboard:{writeText:async code=>copied.push(code)}},
    location:{hash,href:`https://pen.kiiko.ru/${hash}`},document,
    localStorage:{getItem(key){storage.push(['get',key]);if(storageDenied)throw Error('Storage disabled');return storedLanguage;},
      setItem(key,value){storage.push(['set',key,value]);if(storageDenied)throw Error('Storage disabled');}},
    fetch:async(url,options)=>{requests.push({url,options});return {ok:true,json:async()=>({assets})};},
    AbortSignal:{timeout:()=>null},addEventListener:(name,handler)=>events[name]=handler
  });
  await new Promise(resolve=>setImmediate(resolve));
  return {links,heading,document,meta,requests,storage,copied,events};
}

test('Arch download resolves the current x86_64 package in every page language',async()=>{
  const url='https://github.com/Xekep/NovaPen/releases/download/v1.2.3/novapen-1.2.3-1-x86_64.pkg.tar.zst';
  for(const language of ['en','ru','fr','de']) {
    const {links,heading}=await downloadPage(language,[
      {name:'novapen-1.2.3-1-x86_64.pkg.tar.zst.sha256',browser_download_url:`${url}.sha256`},
      {name:'novapen-1.2.3-1-x86_64.pkg.tar.zst',browser_download_url:url}
    ]);
    assert.equal(links.arch.href,url);
    assert.ok(heading.textContent.includes('NovaPen'));
  }
});

test('Arch download keeps the release fallback if metadata has no package or an unrelated URL',async()=>{
  for(const assets of [[],[{name:'novapen-1.2.3-1-x86_64.pkg.tar.zst',browser_download_url:'https://example.com/download'}]]) {
    const {links}=await downloadPage('en',assets);
    assert.equal(links.arch.href,'https://github.com/Xekep/NovaPen/releases/latest');
  }
});

test('Void download selects glibc x86_64 and skips musl, other architectures and checksums',async()=>{
  const url='https://github.com/Xekep/NovaPen/releases/download/v1.2.5/novapen-1.2.5_1.x86_64.xbps';
  for(const language of ['en','ru','fr','de']) {
    const {links}=await downloadPage(language,[
      {name:'novapen-1.2.5_1.x86_64.xbps.sha256',browser_download_url:`${url}.sha256`},
      {name:'novapen-1.2.5_1.x86_64-musl.xbps',browser_download_url:'https://github.com/Xekep/NovaPen/releases/download/v1.2.5/musl.xbps'},
      {name:'novapen-1.2.5_1.aarch64.xbps',browser_download_url:'https://github.com/Xekep/NovaPen/releases/download/v1.2.5/arm.xbps'},
      {name:'novapen-1.2.5_1.x86_64.xbps',browser_download_url:url}
    ]);
    assert.equal(links.void.href,url);
  }
  for(const assets of [[],[{name:'novapen-1.2.5_1.x86_64.xbps',browser_download_url:'https://example.com/download'}]]) {
    const {links}=await downloadPage('en',assets);
    assert.equal(links.void.href,'https://github.com/Xekep/NovaPen/releases/latest');
  }
});

test('language preference persists without storing or requesting the canvas invitation',async()=>{
  const code='ABEiM0RVZneImaq7zN3u_wABAgMEBQYHCAkKCwwNDg8QERITFBUWFxgZGhscHR4f';
  const page=await downloadPage('en',[],{hash:`#${code}`,storedLanguage:'de'});
  assert.equal(page.document.documentElement.lang,'de');
  assert.equal(page.links.invitation.hidden,false);
  await page.links['copy-invite'].onclick();
  assert.deepEqual(page.copied,[code]);
  page.links.language.value='ru';page.links.language.listeners.change();
  assert.equal(page.document.documentElement.lang,'ru');
  assert.deepEqual(page.storage,[['get','novapen.site.language'],['set','novapen.site.language','ru']]);
  assert.equal(page.requests.length,1);
  assert.equal(page.requests[0].url,'https://api.github.com/repos/Xekep/NovaPen/releases/latest');
  assert.equal(page.requests[0].options.credentials,'omit');
  assert.equal(page.requests[0].options.referrerPolicy,'no-referrer');
  assert.ok(!JSON.stringify(page.requests).includes(code));
});

test('page supports Chinese and falls back safely when preference storage is disabled',async()=>{
  const page=await downloadPage('zh-CN',[],{storageDenied:true});
  assert.equal(page.document.documentElement.lang,'zh-Hans');
  assert.ok(page.meta.content.length>0);
  page.links.language.value='fr';assert.doesNotThrow(()=>page.links.language.listeners.change());
  assert.equal(page.document.documentElement.lang,'fr');
  page.events.languagechange();assert.equal(page.document.documentElement.lang,'fr');
});
