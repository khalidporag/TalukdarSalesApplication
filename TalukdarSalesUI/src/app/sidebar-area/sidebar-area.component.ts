import { Component } from '@angular/core';
import { CommonService } from '../services/common/common.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-sidebar-area',
  templateUrl: './sidebar-area.component.html',
  styleUrls: ['./sidebar-area.component.scss']
})
export class SidebarAreaComponent {

  isSidebar: boolean = true;
  isSidebarMobile: boolean = true;

  constructor(private commonService: CommonService, private router: Router) { }

  ngOnInit(): void {
    this.getOpenSidebar();
    this.getOpenSidebarMobile();
  }

  isActive(url: string): any {
    if (this.router.url.includes(`deals-list/details/`)) {
      // this.isActiveDealsDetails = true;
    }
    else {
      // this.isActiveDealsDetails = false;
      return url === this.router.url;
    }
    return url === this.router.url;
  }

  goToPage(slug: any): void {
    this.router.navigate([`${slug}`]);
    this.isSidebarMobile = false;
  }

  getOpenSidebar() {
    this.commonService.sidebarState$.subscribe((open) => {
      this.isSidebar = !this.isSidebar;
    });
  }

  getOpenSidebarMobile() {
    this.commonService.sidebarStateMobile$.subscribe((open) => {
      this.isSidebarMobile = !this.isSidebarMobile;
    });
  }

  closeSidebar() {
    this.isSidebarMobile = false;
  }

  // Sidebar Dropdown
  // Sidebar Dropdown
  expandDropdown(event: any): void {
    let self = event.target;
    let self_parent = self.closest('li');

    // Ensure we are working with the correct element in case of nested elements
    while (self && !self.classList.contains('nav-item__list')) {
      self = self.parentNode;
    }

    if (window.innerWidth > 1) {
      this.getSiblings(self_parent).forEach((item: any) => {
        let children = item.querySelector('.dropdown-area');
        if (children && children.classList.contains('show')) {
          this.closeDropdown(children);
        }
      });

      let dropdown = self_parent.querySelector('.dropdown-area');
      if (dropdown.classList.contains('show')) {
        this.closeDropdown(dropdown);
      } else {
        this.openDropdown(dropdown);
      }
    }
    console.log("CLICKED");
  }

  openDropdown(dropdown: any) {
    let dropdownHeight = 0;
    const dropdownScrollHeight = dropdown.scrollHeight;
    dropdown.style.height = dropdownHeight + 'px';
    dropdown.classList.add('show');

    let expandInterval = setInterval(() => {
      if (dropdownHeight < dropdownScrollHeight) {
        dropdownHeight += 5;
        dropdown.style.height = dropdownHeight + 'px';
      } else {
        dropdown.style.height = '';
        clearInterval(expandInterval);
      }
    }, 1);
  }

  closeDropdown(dropdown: any) {
    let dropdownHeight = dropdown.offsetHeight;

    let collapseInterval = setInterval(() => {
      if (dropdownHeight > 0) {
        dropdownHeight -= 5;
        dropdown.style.height = dropdownHeight + 'px';
      } else {
        dropdown.style.height = '';
        dropdown.classList.remove('show');
        clearInterval(collapseInterval);
      }
    }, 1);
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


}
