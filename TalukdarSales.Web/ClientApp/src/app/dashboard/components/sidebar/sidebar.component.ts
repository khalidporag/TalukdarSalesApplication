import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { CommonService } from 'src/app/services/common/common.service';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
})
export class SidebarComponent {
  isSidebar: boolean = true;
  isSidebarMobile: boolean = true;

  constructor(private commonService: CommonService, private router: Router) { }

  expandDropdown(event: any): void {
    let self = event.target;
    let self_parent = self.closest('li');
    // if (window.innerWidth > 767) {
    if (window.innerWidth > 1) {
      this.getSiblings(self_parent).forEach((item: any) => {
        if (item.classList.contains('drop')) {
          let children = item.querySelector('.dropdown-area');
          if (children.classList.contains('show')) {
            children.classList.remove('show');
          }
        }
      });
      let dropdown = event.target.closest('li').querySelector('ul');
      let dropdownHeight = dropdown.offsetHeight;
      const dropdownScrollHeight = dropdown.scrollHeight;
      if (dropdown.classList.contains('show')) {
        let collapseInterval = setInterval(() => {
          if (dropdownHeight >= 0) {
            dropdownHeight = dropdownHeight - 5;
            dropdown.style.height = dropdownHeight + 'px';
          } else {
            dropdown.removeAttribute('style');
            dropdown.classList.remove('show');
            clearInterval(collapseInterval);
          }
        }, 0);
      } else {
        let collapseInterval = setInterval(() => {
          if (dropdownHeight <= dropdownScrollHeight) {
            dropdownHeight = dropdownHeight + 5;
            dropdown.style.height = dropdownHeight + 'px';
          } else {
            dropdown.removeAttribute('style');
            dropdown.classList.add('show');
            clearInterval(collapseInterval);
          }
        }, 1);
      }
    }
    console.log("CLICKED");
  }

  getSiblings(elem: any) {
    var siblings = [];
    var sibling = elem.parentNode.firstChild;
    while (sibling) {
      if (sibling.nodeType === 1 && sibling !== elem) {
        siblings.push(sibling);
      }
      sibling = sibling.nextSibling;
    }
    return siblings;
  }

  ngOnInit(): void {
    console.log("Onload=======>", this.router.url);
  }

  

}
