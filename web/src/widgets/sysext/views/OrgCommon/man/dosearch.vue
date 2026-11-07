<template>
    <el-checkbox-group :model-value="checkIds" @update:modelValue="onput" class="deptmans">
        <div>
            <el-input v-model="searchValue" placeholder="请搜索职能..." :style="{ width: '200px' }"></el-input>
            <el-button-group>
                <slot name="btn"></slot>
                <el-button :icon="ArrowLeft" @click="pre">上一页</el-button>
                <el-button @click="next">
                    下一页
                </el-button>
            </el-button-group>
            <slot></slot>
        </div>

        <el-checkbox class="checkbox-line" v-for="(man, m) in filteredList" :label="man.KB_DoInfoOID">
            <span>{{ m+1 }}</span>
            <span class="el-icon-s-custom" style="margin-left:16px;margin-right:4px;"></span>
            <span>{{ man.KD_MainName }}</span>
        </el-checkbox>

    </el-checkbox-group>
</template>

<script>
import {
  loadDeptMan
} from "@/api/system/mancommonapi.js"

import { getKBDoInfoByName } from "@k/api/dept/doinfoapi.js"

export default {
  name: 'WebDeptman',
  props: {
      deptOID: null,
      modelValue: null,
  },
  emits: ["change", "update:modelValue"],
  data() {
      return {
          mlist: [],
          checkIds: [],
          map: {},
          pageNum: 1,
          searchValue: '',
      };
  },
  computed: {
      listpara() {
          return {
              deptOID: this.deptOID
          };
      },
      filteredList() {
          return this.mlist.filter(man => {
              return man.KD_MainName.toLowerCase().includes(this.searchValue.toLowerCase());
          });
      }
  },
  mounted() {
      this.getList();
  },
  watch: {
      "listpara": function () {
          this.getList();
      }
  },
  inject:['deptOID'],
  methods: {
      pre() {
          if (this.pageNum > 1) {
              this.pageNum--;
          }
      },
      next() {
          this.pageNum++;
      },
      getList() {
        getKBDoInfoByName(this.listpara).then((res) => {
              this.mlist = res.data;
              var obj = {};
              res.data.forEach((li => {
                  obj[li.KB_DoInfoOID] = li;
              }))
              this.map = obj;
          })
      },
      getChecklist(e) {
          return e.map((KB_DoInfoOID) => this.map[KB_DoInfoOID]);
      },
      onput(e) {
          //console.log(e);
          var list = this.getChecklist(e);
          this.$emit("update:modelValue", list, e);
          this.checkIds = e;
      },
  },
};
</script>

<style scoped>
.deptmans {
  display: grid;
  /* //grid-template-columns:1fr 1fr; */
}

.checkbox-line {
  border-bottom: 1px solid #ccc;
  padding-top: 10px;
  padding-bottom: 10px;
}
</style>
