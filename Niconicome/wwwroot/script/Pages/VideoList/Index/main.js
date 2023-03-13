var r=class{constructor(n,t,e){this.IsSucceeded=n,this.Data=t,this.Message=e}static Succeeded(n){return new r(!0,n,null)}static Fail(n){return new r(!1,null,n)}};var D=class{Get(n){let t;try{t=document.querySelector(n)}catch(e){return r.Fail(`\u8981\u7D20\u3092\u53D6\u5F97\u3067\u304D\u307E\u305B\u3093\u3067\u3057\u305F\u3002(\u8A73\u7D30\uFF1A${e.message})`)}return t==null?r.Fail("\u6307\u5B9A\u3055\u308C\u305F\u8981\u7D20\u304C\u898B\u3064\u304B\u308A\u307E\u305B\u3093\u3002"):r.Succeeded(t)}GetAll(n){let t;try{t=document.querySelectorAll(n)}catch(e){return r.Fail(`\u8981\u7D20\u3092\u53D6\u5F97\u3067\u304D\u307E\u305B\u3093\u3067\u3057\u305F\u3002(\u8A73\u7D30\uFF1A${e.message})`)}return r.Succeeded(t)}};var H=class{constructor(n){this._elmHandler=n}GetComputedStyle(n){let t=this._elmHandler.Get(n);if(!t.IsSucceeded||t.Data===null)return r.Fail(t.Message??"");try{let e=window.getComputedStyle(t.Data);return r.Succeeded(e)}catch{return r.Fail("\u30B9\u30BF\u30A4\u30EB\u3092\u53D6\u5F97\u3067\u304D\u307E\u305B\u3093\u3067\u3057\u305F\u3002")}}};var i=class{};i.VideoListRow=".VideoListRow",i.VideoListRowClassName="VideoListRow",i.VideoListBodyClassName="VideoListBody",i.DropTargetClassName="DropTarget";var g=class{constructor(n,t){this._sourceNiconicoID=null;this._sourceID=null;this._lastOverElement=null;this._elmHandler=n,this._dotnetHelper=t}initialize(){let n=this._elmHandler.GetAll(i.VideoListRow);!n.IsSucceeded||n.Data===null||n.Data.forEach(t=>{t instanceof HTMLElement&&(t.addEventListener("dragstart",e=>{if(!(e.target instanceof HTMLElement))return;let l=this.GetParentByClassName(e.target,i.VideoListRowClassName);l!==null&&(this._sourceNiconicoID=l.dataset.niconicoid??null,this._sourceID=l.id)}),t.addEventListener("dragover",e=>{if(e.preventDefault(),!(e.target instanceof HTMLElement))return;let l=this.GetParentByClassName(e.target,i.VideoListRowClassName);l!==null&&(l.classList.contains(i.DropTargetClassName)||l.classList.add(i.DropTargetClassName),this._lastOverElement=l)}),t.addEventListener("dragleave",e=>{if(e.preventDefault(),!(e.target instanceof HTMLElement))return;let l=this.GetParentByClassName(e.target,i.VideoListRowClassName);l!==null&&l.classList.contains(i.DropTargetClassName)&&l.classList.remove(i.DropTargetClassName)}),t.addEventListener("drop",async e=>{if(e.preventDefault(),this._sourceNiconicoID===null)return;let l=this._elmHandler.Get(`#${this._sourceID}`);if(!l.IsSucceeded||l.Data===null||!(e.target instanceof HTMLElement))return;let a=e.target.parentNode,s=e.target;for(;a!==null;){if(!(a instanceof HTMLElement))return;if(!a.classList.contains(i.VideoListBodyClassName)){s=a,a=a.parentNode;continue}a.insertBefore(l.Data,s),await this._dotnetHelper.invokeMethodAsync("MoveVideo",this._sourceNiconicoID,s.dataset.niconicoid),a=null}this._lastOverElement!==null&&this._lastOverElement.classList.contains(i.DropTargetClassName)&&this._lastOverElement.classList.remove(i.DropTargetClassName)}))})}GetParentByClassName(n,t){let e=n;for(;e!==null;){if(!(e instanceof HTMLElement))return null;if(!e.classList.contains(t)){e=e.parentNode;continue}return e}return null}};var u=class{};u.PageContent=".PageContent",u.VideoListHeader="#VideoListHeader",u.Separator=".Separator";var y=class{constructor(n,t,e){this._isResizing=!1;this._elmHandler=n,this._styleHandler=t,this._dotnetHelper=e,this._columnIDs={0:"CheckBoxColumn",1:"ThumbnailColumn",2:"TitleColumn",3:"UploadedDateTimeColumn",4:"IsDownloadedColumn",5:"ViewCountColumn",6:"CommentCountColumn",7:"MylistCountColumn",8:"LikeCountColumn",9:"MessageColumn"},this._separatorIDs={0:"#CheckBoxColumnSeparator",1:"#ThumbnailColumnSeparator",2:"#TitleColumnSeparator",3:"#UploadedDateTimeColumnSeparator",4:"#IsDownloadedColumnSeparator",5:"#ViewCountColumnSeparator",6:"#CommentCountColumnSeparator",7:"#MylistCountColumnSeparator",8:"#LikeCountColumnSeparator"}}async initialize(){for(let e in this._separatorIDs){let l=this._elmHandler.Get(this._separatorIDs[e]);if(!l.IsSucceeded||l.Data===null)continue;let a=l.Data;if(!(a instanceof HTMLElement))continue;let s=a.dataset.index;s!=null&&a.addEventListener("mousedown",d=>this.OnMouseDown(s))}await this.setWidth();let n=this._elmHandler.Get(u.PageContent);if(!n.IsSucceeded||n.Data===null||!(n.Data instanceof HTMLElement))return;n.Data.addEventListener("mouseup",e=>this.OnMouseUp());let t=this._elmHandler.Get(u.VideoListHeader);!t.IsSucceeded||t.Data===null||!(t.Data instanceof HTMLElement)||t.Data.addEventListener("mousemove",e=>this.OnMouseMove(e))}async setWidth(){let n=0;for(let t in this._columnIDs){let e=null;if(t in this._separatorIDs){let a=this._elmHandler.Get(this._separatorIDs[t]);if(!a.IsSucceeded||a.Data===null||(e=a.Data,!(e instanceof HTMLElement)))continue}let l=this._styleHandler.GetComputedStyle(`#${this._columnIDs[t]}`);if(l.IsSucceeded&&l.Data!==null){let a=l.Data,s=this._elmHandler.GetAll(`.${this._columnIDs[t]}`);if(!s.IsSucceeded||s.Data===null)continue;if(a.display==="none"){e!==null&&(e.style.display="none"),s.Data.forEach(d=>{d instanceof HTMLElement&&(d.style.display="none")});continue}else{let d=await this._dotnetHelper.invokeMethodAsync("GetWidth",this._columnIDs[t]),m=d>0,p=m?d:Number(a.width.match(/\d+/));if(n+=p,e!==null&&(e.style.left=`${n}px`),m){let o=this._elmHandler.Get(`#${this._columnIDs[t]}`);o.IsSucceeded&&o.Data!==null&&o.Data instanceof HTMLElement&&(o.Data.style.width=`${p}px`)}s.Data.forEach(o=>{o instanceof HTMLElement&&(o.style.width=m?`${p}px`:a.width)})}}}}OnMouseDown(n){this._isResizing=!0,this._resizingIndex=n}OnMouseUp(){this._isResizing=!1,this._resizingIndex=null}async OnMouseMove(n){if(!this._isResizing||this._resizingIndex===null)return;let t=Number(this._resizingIndex)+1,e=this._columnIDs[this._resizingIndex],l=this._columnIDs[`${t}`],a=this._elmHandler.Get(`#${e}`),s=this._elmHandler.Get(`#${l}`),d=this._elmHandler.Get(u.VideoListHeader),m=this._elmHandler.GetAll(`.${e}`),p=this._elmHandler.GetAll(`.${l}`),o=this._elmHandler.Get(this._separatorIDs[this._resizingIndex]);if(!a.IsSucceeded||a.Data===null||!m.IsSucceeded||m.Data===null||!o.IsSucceeded||o.Data===null||!d.IsSucceeded||d.Data===null||!s.IsSucceeded||s.Data===null||!p.IsSucceeded||p.Data===null||!(a.Data instanceof HTMLElement)||!(s.Data instanceof HTMLElement))return;let S=a.Data.getBoundingClientRect(),E=d.Data.getBoundingClientRect(),h=n.clientX-S.left,C=h-a.Data.offsetWidth,_=s.Data.offsetWidth-C;if(a.Data.style.width=`${h}px`,s.Data.style.width=`${_}px`,m.Data.forEach(f=>{f instanceof HTMLElement&&(f.style.width=`${h}px`)}),p.Data.forEach(f=>{f instanceof HTMLElement&&(f.style.width=`${_}px`)}),await this._dotnetHelper.invokeMethodAsync("SetWidth",`${h}`,e),await this._dotnetHelper.invokeMethodAsync("SetWidth",`${_}`,l),!(d.Data instanceof HTMLElement)||!(o.Data instanceof HTMLElement))return;let L=S.left-E.left+h-10;o.Data.style.left=`${L}px`}};async function q(c){let n=new D,t=new H(n),e=new y(n,t,c),l=new g(n,c);await e.initialize(),l.initialize()}async function U(c){let n=new D,t=new H(n);await new y(n,t,c).setWidth()}export{q as initialize,U as setWidth};
