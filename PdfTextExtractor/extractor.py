#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
PDF文本提取工具
使用pymupdf从PDF文件中提取文本内容

功能：
1. 扫描版本目录中的guide/Original目录
2. 提取所有PDF文件的文本内容
3. 保存到对应的guide/Utf8Txt目录，保持原有目录结构
"""

import os
import sys
import glob
import pymupdf  # PyMuPDF
from pathlib import Path


class PdfTextExtractor:
    def __init__(self, base_dir=None):
        """
        初始化PDF文本提取器
        
        Args:
            base_dir: 项目根目录，如果不指定则使用脚本所在目录的父目录
        """
        if base_dir is None:
            # 获取脚本所在目录的父目录作为项目根目录
            script_dir = Path(__file__).parent
            self.base_dir = script_dir.parent
        else:
            self.base_dir = Path(base_dir)
        
        print(f"项目根目录: {self.base_dir}")
    
    def find_version_directories(self):
        """
        查找所有版本目录
        
        Returns:
            list: 版本目录列表
        """
        version_dirs = []
        
        # 查找以 v 开头的目录
        pattern = self.base_dir / "v*"
        for path in glob.glob(str(pattern)):
            if os.path.isdir(path):
                version_dirs.append(Path(path))
        
        version_dirs.sort()  # 按名称排序
        print(f"找到版本目录: {[d.name for d in version_dirs]}")
        
        return version_dirs
    
    def extract_text_from_pdf(self, pdf_path):
        """
        从PDF文件中提取文本
        
        Args:
            pdf_path: PDF文件路径
            
        Returns:
            str: 提取的文本内容
        """
        try:
            doc = pymupdf.open(pdf_path)
            text_content = ""
            page_count = len(doc)
            
            for page_num in range(page_count):
                page = doc.load_page(page_num)
                # 只提取文本层内容，不处理图像
                text = page.get_text()
                text_content += f"--- 第 {page_num + 1} 页 ---\n"
                text_content += text + "\n\n"
            
            doc.close()
            
            print(f"成功提取 {pdf_path} 的文本内容，共 {page_count} 页")
            return text_content
            
        except Exception as e:
            print(f"提取 {pdf_path} 时出错: {str(e)}")
            return None
    
    def create_output_directory(self, original_dir, version_dir):
        """
        创建输出目录
        
        Args:
            original_dir: 原始目录路径
            version_dir: 版本目录路径
            
        Returns:
            Path: 输出目录路径
        """
        # 将 guide/Original 替换为 guide/Utf8Txt
        relative_path = original_dir.relative_to(version_dir)
        utf8_path = str(relative_path).replace("guide/Original", "guide/Utf8Txt").replace("guide\\Original", "guide\\Utf8Txt")
        
        output_dir = version_dir / utf8_path
        output_dir.mkdir(parents=True, exist_ok=True)
        
        return output_dir
    
    def process_version_directory(self, version_dir):
        """
        处理单个版本目录
        
        Args:
            version_dir: 版本目录路径
        """
        print(f"\n处理版本目录: {version_dir.name}")
        
        # 查找 guide/Original 目录
        guide_original = version_dir / "guide" / "Original"
        
        if not guide_original.exists():
            print(f"  {version_dir.name} 中没有找到 guide/Original 目录")
            return
        
        # 递归查找所有PDF文件
        pdf_files = list(guide_original.rglob("*.pdf"))
        
        if not pdf_files:
            print(f"  {version_dir.name} 的 guide/Original 目录中没有找到PDF文件")
            return
        
        print(f"  找到 {len(pdf_files)} 个PDF文件:")
        for pdf_file in pdf_files:
            print(f"    {pdf_file.relative_to(version_dir)}")
        
        # 处理每个PDF文件
        for pdf_file in pdf_files:
            self.process_single_pdf(pdf_file, version_dir, guide_original)
    
    def process_single_pdf(self, pdf_file, version_dir, guide_original):
        """
        处理单个PDF文件
        
        Args:
            pdf_file: PDF文件路径
            version_dir: 版本目录路径
            guide_original: guide/Original目录路径
        """
        print(f"\n  处理PDF文件: {pdf_file.name}")
        
        # 提取文本
        text_content = self.extract_text_from_pdf(pdf_file)
        if text_content is None:
            return
        
        # 计算相对路径
        relative_pdf = pdf_file.relative_to(guide_original)
        
        # 创建输出目录
        output_base = version_dir / "guide" / "Utf8Txt"
        if relative_pdf.parent != Path("."):
            # 如果PDF在子目录中，创建相应的子目录
            output_dir = output_base / relative_pdf.parent
            output_dir.mkdir(parents=True, exist_ok=True)
        else:
            output_dir = output_base
            output_dir.mkdir(parents=True, exist_ok=True)
        
        # 生成输出文件名（替换扩展名为.txt）
        output_filename = relative_pdf.stem + ".txt"
        output_file = output_dir / output_filename
        
        # 保存文本内容
        try:
            with open(output_file, 'w', encoding='utf-8') as f:
                f.write(f"PDF文件: {pdf_file.name}\n")
                f.write(f"提取时间: {__import__('datetime').datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
                f.write("=" * 50 + "\n\n")
                f.write(text_content)
            
            print(f"    文本已保存到: {output_file.relative_to(version_dir)}")
            
        except Exception as e:
            print(f"    保存文件时出错: {str(e)}")
    
    def run(self):
        """
        运行主程序
        """
        print("开始PDF文本提取...")
        print("=" * 50)
        
        # 查找版本目录
        version_dirs = self.find_version_directories()
        
        if not version_dirs:
            print("没有找到任何版本目录")
            return
        
        # 处理每个版本目录
        for version_dir in version_dirs:
            self.process_version_directory(version_dir)
        
        print("\n" + "=" * 50)
        print("PDF文本提取完成!")


def main():
    """
    主函数
    """
    try:
        # 创建提取器实例
        extractor = PdfTextExtractor()
        
        # 运行提取程序
        extractor.run()
        
    except KeyboardInterrupt:
        print("\n程序被用户中断")
    except Exception as e:
        print(f"程序运行出错: {str(e)}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    main()
